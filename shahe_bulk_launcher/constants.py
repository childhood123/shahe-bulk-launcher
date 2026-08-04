import os
import re
from pathlib import Path


SANDBOX_INTERNAL_NAME_PATTERN = re.compile(r"^[A-Za-z0-9_]{1,32}$")
OLD_DEFAULT_SANDBOX_ROOT = r"C:\Sandbox\Administrator"
APP_VERSION = "4.0.46"
UPDATE_MANIFEST_URL = "https://lqh.viyf.net/shahe-bulk-launcher/update.json"
LOG_OUTPUT_OFF = "off"
LOG_OUTPUT_DETAILED = "detailed"
LOG_OUTPUT_CHOICES = (
    (LOG_OUTPUT_OFF, "关闭日志输出"),
    (LOG_OUTPUT_DETAILED, "详细日志输出"),
)
FIXED_HOTKEY_TEXT_BY_ACTION = {
    "select_all": "Ctrl+A",
    "open_app": "Ctrl+O",
    "paste_app": "Ctrl+V",
    "refresh": "F5",
    "minimize_to_tray": "Esc",
    "exit_app": "Alt+F4",
}
FIXED_HOTKEY_LABEL_BY_ACTION = {
    "select_all": "全选",
    "open_app": "选择应用",
    "paste_app": "粘贴 .exe 或文件路径",
    "refresh": "刷新沙盒用户",
    "minimize_to_tray": "最小化到托盘",
    "exit_app": "退出软件",
}


def normalize_log_output_mode(value):
    text = str(value or "").strip()
    if text in ("关闭日志输出", LOG_OUTPUT_OFF):
        return LOG_OUTPUT_OFF
    if text in ("详细日志输出", LOG_OUTPUT_DETAILED):
        return LOG_OUTPUT_DETAILED
    return LOG_OUTPUT_DETAILED


def get_default_sandbox_root():
    drive = str(os.environ.get("SystemDrive") or "C:").rstrip("\\/")
    if not drive:
        drive = "C:"
    user_name = str(os.environ.get("USERNAME") or os.environ.get("USER") or "Administrator").strip()
    if not user_name:
        user_name = "Administrator"
    return str(Path(drive + "\\") / "Sandbox" / user_name)


def normalize_sandbox_root_path(sandbox_root):
    value = str(sandbox_root or "").strip()
    if not value:
        return ""
    try:
        return os.path.abspath(os.path.normpath(value))
    except Exception:
        return value


def get_internal_sandbox_name_validation_error(sandbox_name):
    name = str(sandbox_name or "").strip()
    if not name:
        return "内部沙盒名不能为空"
    if len(name) > 32:
        return "内部沙盒名不能超过 32 个字符"
    if name.lower() == "globalsettings" or name.lower().startswith("usersettings_"):
        return "内部沙盒名不能使用 Sandboxie 保留分节名"
    if re.fullmatch(r"(?i)(aux|clock\$|con|nul|prn|com[0-9]|lpt[0-9])", name):
        return "内部沙盒名不能使用 Windows 设备保留名"
    if not SANDBOX_INTERNAL_NAME_PATTERN.fullmatch(name):
        return "内部沙盒名只能使用英文字母、数字和下划线"
    return ""


def is_valid_internal_sandbox_name(sandbox_name):
    return not get_internal_sandbox_name_validation_error(sandbox_name)


def get_sandbox_display_name_validation_error(sandbox_name):
    name = str(sandbox_name or "").strip()
    if not name:
        return "沙盒名不能为空"
    if len(name) > 240:
        return "沙盒名过长"
    if re.search(r"[,()\r\n\t\x00]", name):
        return "沙盒名不能包含英文逗号、括号或控制字符"
    return ""


def make_default_sandbox_display_prefix(prefix="", fallback="沙盒用户"):
    value = re.sub(r"[,()\r\n\t\x00]", "", str(prefix or "").strip())
    value = re.sub(r"\d+$", "", value)
    if not value:
        value = str(fallback or "沙盒用户")
    return value[:80] or "沙盒用户"
