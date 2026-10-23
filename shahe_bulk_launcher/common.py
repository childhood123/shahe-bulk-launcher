import base64
import configparser
import ctypes
import hashlib
import json
import os
import re
import subprocess
import sys
import time
import ssl
import urllib.error
import urllib.request
from pathlib import Path

UPDATE_PUBLIC_KEY_B64 = "imOoLvH7P4q5ObQVX/T1Fo+JoZdunmRtLDV4CwMFO9w="
UPDATE_PACKAGE_KEY_B64 = "QR9QUEWXC4Ey0J6kVTyMiC/CQQOMBPtJcBT2Qk0W2xs="
UPDATE_ENCRYPTED_MAGIC = b"SHAHEUPD1"
CONFIG_STORE_NAME = "shahe_config.ini"
CONFIG_STORE_SECTION = "config"
CONFIG_SETTINGS_SECTION = "settings"
CONFIG_SECRET_SALT = b"shahe-bulk-launcher-config-v1"
UPDATE_TEMP_DIR_NAME = "tenp"


class _AllowedHttpErrorResponse:
    def __init__(self, error):
        self._error = error
        self.status = int(getattr(error, "code", 0) or 0)
        self.headers = getattr(error, "headers", {})

    def read(self, *args, **kwargs):
        return self._error.read(*args, **kwargs)

    def close(self):
        try:
            self._error.close()
        except Exception:
            pass

    def __enter__(self):
        return self

    def __exit__(self, exc_type, exc, tb):
        self.close()
        return False


def is_allowed_update_body_status(status, url=""):
    try:
        status = int(status)
    except Exception:
        return False
    allowed_url = str(url or "").strip().lower()
    if allowed_url:
        return status == 404 and allowed_url.startswith("https://lqh.viyf.net/")
    return status == 404


def open_update_url(request, timeout=20, allow_error_body=False):
    url = request.full_url if isinstance(request, urllib.request.Request) else str(request)
    try:
        return urllib.request.urlopen(request, timeout=timeout)
    except urllib.error.HTTPError as error:
        if allow_error_body and is_allowed_update_body_status(getattr(error, "code", 0), url):
            return _AllowedHttpErrorResponse(error)
        raise


def parse_bool_setting(value, default=False):
    if isinstance(value, str):
        text = value.strip().lower()
        if text in ("1", "true", "yes", "y", "on", "是", "开", "开启"):
            return True
        if text in ("0", "false", "no", "n", "off", "否", "关", "关闭", ""):
            return False
        return bool(default)
    if value is None:
        return bool(default)
    return bool(value)


def parse_int_setting(value, default=0, min_value=None, max_value=None):
    try:
        number = int(value)
    except Exception:
        number = int(default)
    if min_value is not None:
        number = max(int(min_value), number)
    if max_value is not None:
        number = min(int(max_value), number)
    return number


def end_dialog_safely(dialog, result_code):
    try:
        if dialog.IsModal():
            dialog.EndModal(result_code)
        else:
            dialog.Close()
    except RuntimeError:
        pass


def parse_version_parts(version_text):
    parts = []
    for part in re.split(r"[^\d]+", str(version_text or "").strip()):
        if part == "":
            continue
        try:
            parts.append(int(part))
        except Exception:
            parts.append(0)
    return parts or [0]


def compare_versions(left, right):
    left_parts = parse_version_parts(left)
    right_parts = parse_version_parts(right)
    max_len = max(len(left_parts), len(right_parts))
    left_parts.extend([0] * (max_len - len(left_parts)))
    right_parts.extend([0] * (max_len - len(right_parts)))
    for left_value, right_value in zip(left_parts, right_parts):
        if left_value > right_value:
            return 1
        if left_value < right_value:
            return -1
    return 0


def is_packaged_app():
    return bool(globals().get("__compiled__", False)) or os.environ.get("SHAHE_PACKAGED_APP") == "1"


def get_current_executable_path():
    if is_packaged_app():
        script_path = str(getattr(sys, "argv", [""])[0] or "").strip()
        if script_path:
            executable_path = os.path.abspath(script_path)
            executable_dir = os.path.dirname(executable_path)
            if os.path.basename(executable_dir).lower() == "internal":
                public_executable = os.path.join(os.path.dirname(executable_dir), os.path.basename(executable_path))
                if os.path.exists(public_executable):
                    return public_executable
            return executable_path
    script_path = str(getattr(sys, "argv", [""])[0] or "").strip()
    if script_path and script_path not in ("-c", "-m"):
        return os.path.abspath(script_path)
    main_module = sys.modules.get("__main__")
    main_file = str(getattr(main_module, "__file__", "") or "").strip()
    if main_file:
        return os.path.abspath(main_file)
    return os.path.abspath(__file__)


def get_app_dir():
    if is_packaged_app():
        executable_dir = os.path.dirname(os.path.abspath(str(getattr(sys, "argv", [""])[0] or "")))
        if os.path.basename(executable_dir).lower() == "internal":
            return os.path.dirname(executable_dir)
        return os.path.dirname(get_current_executable_path())
    script_path = str(getattr(sys, "argv", [""])[0] or "").strip()
    if script_path and script_path not in ("-c", "-m"):
        return os.path.dirname(os.path.abspath(script_path))
    main_module = sys.modules.get("__main__")
    main_file = str(getattr(main_module, "__file__", "") or "").strip()
    if main_file:
        return os.path.dirname(os.path.abspath(main_file))
    return os.path.dirname(os.path.abspath(__file__))


def get_resource_path(relative_path):
    return os.path.join(get_app_dir(), relative_path)


def get_configuration_dir(base_dir=None):
    root = str(base_dir or get_app_dir() or "").strip()
    if not root:
        root = get_app_dir()
    return os.path.join(root, "Configurations")


def prepare_configuration_dir(base_dir=None):
    directory = get_configuration_dir(base_dir)
    previous = os.path.join(os.path.dirname(directory), "Configuration")
    if os.path.isdir(previous) and (not os.path.exists(directory) or not os.listdir(directory)):
        if os.path.isdir(directory):
            os.rmdir(directory)
        os.rename(previous, directory)
    os.makedirs(directory, exist_ok=True)
    return directory


def get_configuration_db_path(base_dir=None):
    return os.path.join(get_configuration_dir(base_dir), CONFIG_STORE_NAME)


def get_update_temp_dir(base_dir=None):
    root = str(base_dir or get_app_dir() or "").strip()
    if not root:
        root = get_app_dir()
    return os.path.join(root, UPDATE_TEMP_DIR_NAME)


def _get_config_fingerprint():
    parts = []
    if os.name == "nt":
        try:
            import winreg

            with winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, r"SOFTWARE\Microsoft\Cryptography") as key:
                machine_guid, _ = winreg.QueryValueEx(key, "MachineGuid")
                if machine_guid:
                    parts.append(str(machine_guid).strip())
        except Exception:
            pass
    for candidate in (
        os.environ.get("COMPUTERNAME"),
        os.environ.get("USERNAME"),
        os.environ.get("USERDOMAIN"),
        os.environ.get("PROCESSOR_IDENTIFIER"),
    ):
        value = str(candidate or "").strip()
        if value:
            parts.append(value)
    if not parts:
        parts.append("shahe-bulk-launcher")
    return "|".join(parts)


def _build_config_key():
    seed = f"{_get_config_fingerprint()}|shahe-bulk-launcher|config-v1"
    return hashlib.sha256(seed.encode("utf-8", errors="ignore")).digest()


def encrypt_config_value(value):
    text = str(value or "")
    if not text:
        return ""
    try:
        from cryptography.hazmat.primitives.ciphers.aead import AESGCM

        nonce = os.urandom(12)
        ciphertext = AESGCM(_build_config_key()).encrypt(nonce, text.encode("utf-8"), None)
        return base64.b64encode(nonce + ciphertext).decode("ascii")
    except Exception as exc:
        raise RuntimeError(f"配置值加密失败: {exc}")


def decrypt_config_value(value):
    text = str(value or "").strip()
    if not text:
        return ""
    try:
        from cryptography.hazmat.primitives.ciphers.aead import AESGCM

        raw = base64.b64decode(text)
        if len(raw) <= 12:
            return ""
        nonce = raw[:12]
        ciphertext = raw[12:]
        plain = AESGCM(_build_config_key()).decrypt(nonce, ciphertext, None)
        return plain.decode("utf-8")
    except Exception:
        return ""


class ConfigStore:
    def __init__(self, store_path):
        self.store_path = str(store_path or "").strip()

    def _load(self):
        if not self.store_path:
            raise RuntimeError("配置文件路径无效")
        path = Path(self.store_path)
        path.parent.mkdir(parents=True, exist_ok=True)
        parser = configparser.ConfigParser(interpolation=None)
        parser.optionxform = str
        if not path.exists():
            return parser
        try:
            parser.read(path, encoding="utf-8")
        except Exception:
            parser = configparser.ConfigParser(interpolation=None)
            parser.optionxform = str
        if not parser.has_section(CONFIG_STORE_SECTION):
            parser.add_section(CONFIG_STORE_SECTION)
        return parser

    def _save(self, parser):
        if not self.store_path:
            raise RuntimeError("配置文件路径无效")
        path = Path(self.store_path)
        path.parent.mkdir(parents=True, exist_ok=True)
        if not parser.has_section(CONFIG_STORE_SECTION):
            parser.add_section(CONFIG_STORE_SECTION)
        temp_path = path.with_suffix(path.suffix + ".tmp")
        with open(temp_path, "w", encoding="utf-8") as file:
            parser.write(file, space_around_delimiters=False)
        os.replace(temp_path, path)

    def get_text(self, key, default=""):
        key = str(key or "").strip()
        if not key:
            return default
        try:
            parser = self._load()
            encrypted_key = key if key.endswith("_encrypted") else f"{key}_encrypted"
            if parser.has_option(CONFIG_STORE_SECTION, encrypted_key):
                value = parser.get(CONFIG_STORE_SECTION, encrypted_key, fallback="")
                text = decrypt_config_value(value)
                return text if text else default
            if not parser.has_option(CONFIG_STORE_SECTION, key):
                return default
            value = parser.get(CONFIG_STORE_SECTION, key, fallback="")
            return str(value or "")
        except Exception:
            return default

    def set_text(self, key, value, secret=False):
        key = str(key or "").strip()
        if not key:
            return False
        try:
            storage_key = f"{key}_encrypted" if secret else key
            remove_key = key if secret else f"{key}_encrypted"
            text = encrypt_config_value(value) if secret else str(value or "")
            parser = self._load()
            if not parser.has_section(CONFIG_STORE_SECTION):
                parser.add_section(CONFIG_STORE_SECTION)
            if parser.has_option(CONFIG_STORE_SECTION, remove_key):
                parser.remove_option(CONFIG_STORE_SECTION, remove_key)
            parser.set(CONFIG_STORE_SECTION, storage_key, text)
            parser.set(CONFIG_STORE_SECTION, f"{storage_key}_updated_at", str(int(time.time())))
            self._save(parser)
            return True
        except Exception:
            return False

    def get_json(self, key, default=None):
        if str(key or "").strip() == "settings_json":
            try:
                parser = self._load()
                if parser.has_section(CONFIG_SETTINGS_SECTION) and parser.items(CONFIG_SETTINGS_SECTION):
                    settings = {}
                    for name, value in parser.items(CONFIG_SETTINGS_SECTION):
                        if str(name or "").endswith("_updated_at"):
                            continue
                        try:
                            settings[name] = json.loads(value)
                        except Exception:
                            settings[name] = value
                    return settings
            except Exception:
                pass
        text = self.get_text(key, "")
        if not text:
            return default
        try:
            return json.loads(text)
        except Exception:
            return default

    def set_json(self, key, value):
        if str(key or "").strip() == "settings_json":
            if not isinstance(value, dict):
                return False
            try:
                parser = self._load()
                if parser.has_section(CONFIG_SETTINGS_SECTION):
                    parser.remove_section(CONFIG_SETTINGS_SECTION)
                parser.add_section(CONFIG_SETTINGS_SECTION)
                if parser.has_option(CONFIG_STORE_SECTION, "settings_json"):
                    parser.remove_option(CONFIG_STORE_SECTION, "settings_json")
                if parser.has_option(CONFIG_STORE_SECTION, "settings_json_updated_at"):
                    parser.remove_option(CONFIG_STORE_SECTION, "settings_json_updated_at")
                for setting_key in sorted(value):
                    if not str(setting_key or "").strip():
                        continue
                    parser.set(
                        CONFIG_SETTINGS_SECTION,
                        str(setting_key),
                        json.dumps(value[setting_key], ensure_ascii=False, separators=(",", ":"))
                    )
                parser.set(CONFIG_SETTINGS_SECTION, "updated_at", str(int(time.time())))
                self._save(parser)
                return True
            except Exception:
                return False
        try:
            text = json.dumps(value, ensure_ascii=False, indent=2)
        except Exception:
            return False
        return self.set_text(key, text, secret=False)

    def get_secret_text(self, key, default=""):
        return self.get_text(key, default=default)

    def set_secret_text(self, key, value):
        return self.set_text(key, value, secret=True)


def get_hidden_subprocess_kwargs():
    kwargs = {}
    if os.name != "nt":
        return kwargs
    try:
        startupinfo = subprocess.STARTUPINFO()
        startupinfo.dwFlags |= getattr(subprocess, "STARTF_USESHOWWINDOW", 1)
        startupinfo.wShowWindow = 0
        kwargs["startupinfo"] = startupinfo
    except Exception:
        pass
    creationflags = getattr(subprocess, "CREATE_NO_WINDOW", 0)
    if creationflags:
        kwargs["creationflags"] = creationflags
    return kwargs


def format_size(num_bytes):
    try:
        size = float(num_bytes)
    except Exception:
        size = 0.0
    units = ["B", "KB", "MB", "GB"]
    for unit in units:
        if size < 1024 or unit == units[-1]:
            if unit == "B":
                return f"{int(size)} {unit}"
            return f"{size:.1f} {unit}"
        size /= 1024.0
    return f"{int(num_bytes)} B"


class UpdateError(RuntimeError):
    pass


def verify_update_signature(file_path, signature_b64):
    signature_b64 = str(signature_b64 or "").strip()
    if not signature_b64:
        raise UpdateError("更新包缺少签名，已停止安装")
    try:
        from cryptography.hazmat.primitives.asymmetric.ed25519 import Ed25519PublicKey

        public_key = Ed25519PublicKey.from_public_bytes(base64.b64decode(UPDATE_PUBLIC_KEY_B64))
        signature = base64.b64decode(signature_b64)
        with open(file_path, "rb") as f:
            data = f.read()
        public_key.verify(signature, data)
    except UpdateError:
        raise
    except Exception as exc:
        raise UpdateError(f"更新包签名校验失败，已停止安装: {exc}")


def decrypt_update_package(encrypted_path, zip_path):
    try:
        from cryptography.hazmat.primitives.ciphers.aead import AESGCM

        data = Path(encrypted_path).read_bytes()
        if not data.startswith(UPDATE_ENCRYPTED_MAGIC):
            raise UpdateError("更新包加密格式无效")
        pos = len(UPDATE_ENCRYPTED_MAGIC)
        nonce = data[pos:pos + 12]
        ciphertext = data[pos + 12:]
        if len(nonce) != 12 or not ciphertext:
            raise UpdateError("更新包加密格式无效")
        key = base64.b64decode(UPDATE_PACKAGE_KEY_B64)
        plain = AESGCM(key).decrypt(nonce, ciphertext, None)
        Path(zip_path).write_bytes(plain)
    except UpdateError:
        raise
    except Exception as exc:
        raise UpdateError(f"更新包解密失败，已停止安装: {exc}")
