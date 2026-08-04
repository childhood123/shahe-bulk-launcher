import ast
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "沙河批量打开器.py"
ASSIGNMENTS = {
    "SANDBOX_INTERNAL_NAME_PATTERN",
    "OLD_DEFAULT_SANDBOX_ROOT",
    "APP_VERSION",
    "UPDATE_MANIFEST_URL",
    "LOG_OUTPUT_OFF",
    "LOG_OUTPUT_DETAILED",
    "LOG_OUTPUT_CHOICES",
    "FIXED_HOTKEY_TEXT_BY_ACTION",
    "FIXED_HOTKEY_LABEL_BY_ACTION",
}
FUNCTIONS = {
    "normalize_log_output_mode",
    "get_default_sandbox_root",
    "normalize_sandbox_root_path",
    "get_internal_sandbox_name_validation_error",
    "is_valid_internal_sandbox_name",
    "get_sandbox_display_name_validation_error",
    "make_default_sandbox_display_prefix",
    "is_packaged_app",
    "get_current_executable_path",
    "get_resource_path",
    "get_app_dir",
    "cleanup_legacy_release_artifacts",
    "append_daily_log",
}
IMPORT_BLOCK = (
    "from shahe_bulk_launcher.bootstrap import (\n"
    "    append_daily_log,\n"
    "    cleanup_legacy_release_artifacts,\n"
    "    get_app_dir,\n"
    "    get_current_executable_path,\n"
    "    get_resource_path,\n"
    "    is_packaged_app,\n"
    ")\n"
    "from shahe_bulk_launcher.constants import (\n"
    "    APP_VERSION,\n"
    "    FIXED_HOTKEY_LABEL_BY_ACTION,\n"
    "    FIXED_HOTKEY_TEXT_BY_ACTION,\n"
    "    LOG_OUTPUT_CHOICES,\n"
    "    LOG_OUTPUT_DETAILED,\n"
    "    LOG_OUTPUT_OFF,\n"
    "    OLD_DEFAULT_SANDBOX_ROOT,\n"
    "    SANDBOX_INTERNAL_NAME_PATTERN,\n"
    "    UPDATE_MANIFEST_URL,\n"
    "    get_default_sandbox_root,\n"
    "    get_internal_sandbox_name_validation_error,\n"
    "    get_sandbox_display_name_validation_error,\n"
    "    is_valid_internal_sandbox_name,\n"
    "    make_default_sandbox_display_prefix,\n"
    "    normalize_log_output_mode,\n"
    "    normalize_sandbox_root_path,\n"
    ")\n"
    "from shahe_bulk_launcher.single_instance import SingleInstance\n\n"
)


def _assigned_names(node):
    if not isinstance(node, (ast.Assign, ast.AnnAssign)):
        return set()
    targets = node.targets if isinstance(node, ast.Assign) else [node.target]
    return {target.id for target in targets if isinstance(target, ast.Name)}


def main():
    text = SOURCE.read_text(encoding="utf-8-sig")
    lines = text.splitlines(keepends=True)
    tree = ast.parse(text)
    removals = []
    removed_assignments = set()
    removed_functions = set()
    removed_single_instance = False
    import_anchor = None

    for node in tree.body:
        names = _assigned_names(node)
        matched_assignments = names & ASSIGNMENTS
        if matched_assignments:
            removals.append((node.lineno, node.end_lineno))
            removed_assignments.update(matched_assignments)
        elif isinstance(node, (ast.FunctionDef, ast.AsyncFunctionDef)) and node.name in FUNCTIONS:
            removals.append((node.lineno, node.end_lineno))
            removed_functions.add(node.name)
        elif isinstance(node, ast.ClassDef) and node.name == "SingleInstance":
            removals.append((node.lineno, node.end_lineno))
            removed_single_instance = True
        elif isinstance(node, ast.If) and node.lineno == 82:
            import_anchor = node.end_lineno

    if removed_assignments != ASSIGNMENTS:
        raise RuntimeError(f"Assignment mismatch: {ASSIGNMENTS - removed_assignments}")
    if removed_functions != FUNCTIONS:
        raise RuntimeError(f"Function mismatch: {FUNCTIONS - removed_functions}")
    if not removed_single_instance or import_anchor is None:
        raise RuntimeError("Entry structure no longer matches the foundation migration baseline")

    for start, end in sorted(removals, reverse=True):
        del lines[start - 1:end]
        if start <= import_anchor:
            import_anchor -= end - start + 1

    lines.insert(import_anchor, "\n" + IMPORT_BLOCK)
    SOURCE.write_text("".join(lines), encoding="utf-8", newline="")


if __name__ == "__main__":
    main()
