# 用户指定打赏资源接入

用户提供支付宝、微信两张原图，要求位于软件根目录“打赏”，从现有菜单显示，复制按钮复制图片本身。保留现有菜单、对话框中文文案、缩放和键盘操作。

映射：根目录打赏 JPG → App 项目内容复制 → UpdateService.ResolveQrImagePath 优先读取打赏目录 → MainForm.ShowQrCodeDialog → QRCodeDialog.CopyQrCode → DialogImageUtilities.CopyImageToClipboard 的 Clipboard.SetImage。

本轮只补资源和读取/构建接线，复用既有图片复制实现；不修改 Python 或活动迁移计划。验证覆盖目录优先级、两张发布原图、真实按钮复制图像尺寸/像素、已有入口与对话框回归、C#/Python 全量和 Release 构建。
