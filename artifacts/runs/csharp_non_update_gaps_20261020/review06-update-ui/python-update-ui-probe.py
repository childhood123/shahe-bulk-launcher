from pathlib import Path
source = Path('shahe_bulk_launcher/features/updates.py').read_text(encoding='utf-8-sig')
checks = {
    'manual_log': '手动检查更新' in source,
    'manual_hide_notice': '_show_update_notice(False)' in source,
    'checking_status': '正在检查更新...' in source,
    'prepare_status': '正在准备更新程序...' in source,
    'unavailable_status': '自动更新不可用，请稍后重试' in source,
    'started_status': '更新程序已启动，正在关闭主程序...' in source,
    'failed_status': '自动更新启动失败，请稍后重试' in source,
}
assert all(checks.values()), checks
print(f"PASS: {len(checks)} Python update UI source checks; failed=0 skipped=0")
