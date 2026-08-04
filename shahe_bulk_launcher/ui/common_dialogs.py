import os

import wx

from ..common import end_dialog_safely
from .controls import CheckableListCtrl


def _load_wx_image_from_bytes(image_bytes):
    data = bytes(image_bytes or b"")
    if not data:
        return wx.Image()
    try:
        stream = wx.MemoryInputStream(data, len(data))
        return wx.Image(stream, wx.BITMAP_TYPE_ANY)
    except Exception:
        return wx.Image()

def copy_image_to_clipboard(img_path, parent=None):
    if not os.path.exists(img_path):
        wx.MessageBox(f"未找到图片文件：{img_path}", "错误", wx.OK | wx.ICON_ERROR, parent)
        return False
    try:
        img = wx.Image(img_path, wx.BITMAP_TYPE_ANY)
        bmp = wx.Bitmap(img)
        if wx.TheClipboard.Open():
            wx.TheClipboard.SetData(wx.BitmapDataObject(bmp))
            wx.TheClipboard.Close()
            wx.MessageBox("二维码已复制到剪贴板！", "成功", wx.OK | wx.ICON_INFORMATION, parent)
            return True
        else:
            wx.MessageBox("无法打开剪贴板", "错误", wx.OK | wx.ICON_ERROR, parent)
            return False
    except Exception as e:
        wx.MessageBox(f"复制失败：{str(e)}", "错误", wx.OK | wx.ICON_ERROR, parent)
        return False

class QRCodeDialog(wx.Dialog):
    def __init__(self, parent, title, img_path="", embedded_bytes=None, copy_img_path=""):
        super().__init__(parent, title=title, size=(500, 600))
        self.img_path = img_path
        self.embedded_bytes = embedded_bytes
        self.copy_img_path = str(copy_img_path or "").strip()
        self.init_ui()
        self.Centre()

    def init_ui(self):
        panel = wx.Panel(self)
        sizer = wx.BoxSizer(wx.VERTICAL)
        try:
            if self.embedded_bytes:
                img = _load_wx_image_from_bytes(self.embedded_bytes)
            else:
                img = wx.Image(self.img_path, wx.BITMAP_TYPE_ANY)
            if not img.IsOk():
                raise RuntimeError("二维码资源加载失败")
            max_size = 450
            width, height = img.GetWidth(), img.GetHeight()
            if width > max_size or height > max_size:
                scale = max_size / max(width, height)
                new_w = int(width * scale)
                new_h = int(height * scale)
                img = img.Scale(new_w, new_h, wx.IMAGE_QUALITY_HIGH)
            bmp = wx.Bitmap(img)
            self.static_bmp = wx.StaticBitmap(panel, bitmap=bmp)
            sizer.Add(self.static_bmp, 0, wx.ALL | wx.CENTER, 10)
        except Exception as e:
            wx.MessageBox(f"无法加载图片：{str(e)}", "错误", wx.OK | wx.ICON_ERROR, self)
            self.Close()
            return
        tip = wx.StaticText(panel, label="用手机微信扫码请作者吃辣条")
        tip.SetFont(wx.Font(10, wx.FONTFAMILY_DEFAULT, wx.FONTSTYLE_NORMAL, wx.FONTWEIGHT_NORMAL))
        sizer.Add(tip, 0, wx.ALL | wx.CENTER, 5)
        btn_sizer = wx.BoxSizer(wx.HORIZONTAL)
        copy_btn = wx.Button(panel, label="复制二维码", size=(120, 35))
        copy_btn.Bind(wx.EVT_BUTTON, self.on_copy)
        btn_sizer.Add(copy_btn, 0, wx.ALL, 10)
        close_btn = wx.Button(panel, label="关闭", size=(80, 35))
        close_btn.Bind(wx.EVT_BUTTON, lambda e: self.Close())
        btn_sizer.Add(close_btn, 0, wx.ALL, 10)
        sizer.Add(btn_sizer, 0, wx.CENTER)
        panel.SetSizer(sizer)

    def on_copy(self, event):
        copy_target = self.copy_img_path or self.img_path
        if not copy_target:
            wx.MessageBox("未找到 internal 二维码文件，无法复制", "错误", wx.OK | wx.ICON_ERROR, self)
            return
        copy_image_to_clipboard(copy_target, self)

class SelectUsersDialog(wx.Dialog):
    """用于批量选择沙盒用户的对话框，使用 CheckableListCtrl"""
    def __init__(self, parent, title, users):
        super().__init__(parent, title=title, size=(400, 500))
        self.users = users
        self.selected_users = []
        self.init_ui()

    def init_ui(self):
        panel = wx.Panel(self)
        sizer = wx.BoxSizer(wx.VERTICAL)
        # 提示标签
        label = wx.StaticText(panel, label="请选择沙盒用户（可多选）：")
        sizer.Add(label, 0, wx.ALL, 5)
        # 可复选列表
        self.list_ctrl = CheckableListCtrl(panel)
        for user in self.users:
            self.list_ctrl.append_item(user)
        sizer.Add(self.list_ctrl, 1, wx.EXPAND | wx.ALL, 5)
        # 按钮
        btn_sizer = wx.BoxSizer(wx.HORIZONTAL)
        ok_btn = wx.Button(panel, label="确定")
        ok_btn.Bind(wx.EVT_BUTTON, self.on_ok)
        cancel_btn = wx.Button(panel, label="取消")
        cancel_btn.Bind(wx.EVT_BUTTON, lambda e: end_dialog_safely(self, wx.ID_CANCEL))
        btn_sizer.Add(ok_btn, 0, wx.ALL, 5)
        btn_sizer.Add(cancel_btn, 0, wx.ALL, 5)
        sizer.Add(btn_sizer, 0, wx.ALIGN_CENTER | wx.ALL, 5)
        panel.SetSizer(sizer)
        self.Centre()

    def on_ok(self, event):
        checked_indices = self.list_ctrl.get_checked_items()
        self.selected_users = [self.users[i] for i in checked_indices]
        end_dialog_safely(self, wx.ID_OK)

    def get_selected_users(self):
        return self.selected_users

class GroupNameDialog(wx.Dialog):
    def __init__(self, parent, title="分组名称", default_value=""):
        super().__init__(parent, title=title, size=(380, 150))
        self.value = ""
        panel = wx.Panel(self)
        sizer = wx.BoxSizer(wx.VERTICAL)
        sizer.Add(wx.StaticText(panel, label="请输入分组名称:"), 0, wx.ALL, 10)
        self.text_name = wx.TextCtrl(panel, value=default_value)
        self.text_name.SetName("分组名称")
        sizer.Add(self.text_name, 0, wx.LEFT | wx.RIGHT | wx.BOTTOM | wx.EXPAND, 10)
        btn_sizer = wx.BoxSizer(wx.HORIZONTAL)
        btn_sizer.AddStretchSpacer()
        btn_ok = wx.Button(panel, label="确定")
        btn_ok.Bind(wx.EVT_BUTTON, self.on_ok)
        btn_sizer.Add(btn_ok, 0, wx.RIGHT, 5)
        btn_cancel = wx.Button(panel, label="取消")
        btn_cancel.Bind(wx.EVT_BUTTON, self.on_cancel)
        btn_sizer.Add(btn_cancel, 0)
        sizer.Add(btn_sizer, 0, wx.EXPAND | wx.ALL, 10)
        panel.SetSizer(sizer)
        self.Centre()

    def on_ok(self, event):
        self.value = self.text_name.GetValue().strip()
        if not self.value:
            return
        end_dialog_safely(self, wx.ID_OK)

    def on_cancel(self, event):
        end_dialog_safely(self, wx.ID_CANCEL)

class BatchNameRuleDialog(wx.Dialog):
    def __init__(
        self,
        parent,
        title,
        prompt,
        default_prefix="用户",
        default_start=1,
        default_digits=0,
        default_count=1,
        fixed_count=None,
        digit_choice_values=None
    ):
        height = 320 if fixed_count is None else 300
        super().__init__(parent, title=title, size=(430, height))
        self.result = None
        self.fixed_count = fixed_count
        self.digit_choice_values = [
            (int(value), str(label or "").strip())
            for value, label in (digit_choice_values or [])
            if str(label or "").strip()
        ]
        panel = wx.Panel(self)
        sizer = wx.BoxSizer(wx.VERTICAL)

        tip = wx.StaticText(panel, label=prompt)
        sizer.Add(tip, 0, wx.ALL | wx.EXPAND, 10)
        example = wx.StaticText(
            panel,
            label="示例：前缀=沙盒用户，起始=1，位数=2，数量=3 -> 沙盒用户01、沙盒用户02、沙盒用户03"
        )
        sizer.Add(example, 0, wx.LEFT | wx.RIGHT | wx.BOTTOM | wx.EXPAND, 10)

        prefix_sizer = wx.BoxSizer(wx.HORIZONTAL)
        prefix_sizer.Add(wx.StaticText(panel, label="名称前缀:"), 0, wx.ALIGN_CENTER_VERTICAL | wx.RIGHT, 8)
        self.text_prefix = wx.TextCtrl(panel, value=str(default_prefix or ""))
        self.text_prefix.SetName("名称前缀")
        prefix_sizer.Add(self.text_prefix, 1)
        sizer.Add(prefix_sizer, 0, wx.LEFT | wx.RIGHT | wx.BOTTOM | wx.EXPAND, 10)

        start_sizer = wx.BoxSizer(wx.HORIZONTAL)
        start_sizer.Add(wx.StaticText(panel, label="起始编号（第一个编号）:"), 0, wx.ALIGN_CENTER_VERTICAL | wx.RIGHT, 8)
        self.spin_start = wx.SpinCtrl(panel, min=0, max=999999, initial=max(0, int(default_start)))
        self.spin_start.SetName("起始编号")
        start_sizer.Add(self.spin_start, 0)
        start_sizer.AddStretchSpacer()
        sizer.Add(start_sizer, 0, wx.LEFT | wx.RIGHT | wx.BOTTOM | wx.EXPAND, 10)

        digits_sizer = wx.BoxSizer(wx.HORIZONTAL)
        if self.digit_choice_values:
            digits_sizer.Add(wx.StaticText(panel, label="编号格式:"), 0, wx.ALIGN_CENTER_VERTICAL | wx.RIGHT, 8)
            self.choice_digits = wx.Choice(panel, choices=[label for _, label in self.digit_choice_values])
            self.choice_digits.SetName("编号格式")
            default_index = 0
            for index, (value, _) in enumerate(self.digit_choice_values):
                if int(value) == int(max(0, int(default_digits))):
                    default_index = index
                    break
            self.choice_digits.SetSelection(default_index)
            digits_sizer.Add(self.choice_digits, 0)
        else:
            digits_sizer.Add(wx.StaticText(panel, label="编号位数（0=不补零，2=01）:"), 0, wx.ALIGN_CENTER_VERTICAL | wx.RIGHT, 8)
            self.spin_digits = wx.SpinCtrl(panel, min=0, max=8, initial=max(0, int(default_digits)))
            self.spin_digits.SetName("编号位数")
            digits_sizer.Add(self.spin_digits, 0)
        digits_sizer.AddStretchSpacer()
        sizer.Add(digits_sizer, 0, wx.LEFT | wx.RIGHT | wx.BOTTOM | wx.EXPAND, 10)

        if fixed_count is None:
            count_sizer = wx.BoxSizer(wx.HORIZONTAL)
            count_sizer.Add(wx.StaticText(panel, label="创建数量（要创建几个用户）:"), 0, wx.ALIGN_CENTER_VERTICAL | wx.RIGHT, 8)
            self.spin_count = wx.SpinCtrl(panel, min=1, max=5000, initial=max(1, int(default_count)))
            self.spin_count.SetName("创建数量")
            count_sizer.Add(self.spin_count, 0)
            count_sizer.AddStretchSpacer()
            sizer.Add(count_sizer, 0, wx.LEFT | wx.RIGHT | wx.BOTTOM | wx.EXPAND, 10)
        else:
            count_sizer = wx.BoxSizer(wx.HORIZONTAL)
            count_sizer.Add(wx.StaticText(panel, label=f"目标数量: {int(max(0, fixed_count))}"), 0, wx.ALIGN_CENTER_VERTICAL)
            count_sizer.AddStretchSpacer()
            sizer.Add(count_sizer, 0, wx.LEFT | wx.RIGHT | wx.BOTTOM | wx.EXPAND, 10)

        btn_sizer = wx.BoxSizer(wx.HORIZONTAL)
        btn_sizer.AddStretchSpacer()
        btn_ok = wx.Button(panel, label="确定")
        btn_ok.Bind(wx.EVT_BUTTON, self.on_ok)
        btn_sizer.Add(btn_ok, 0, wx.RIGHT, 6)
        btn_cancel = wx.Button(panel, label="取消")
        btn_cancel.Bind(wx.EVT_BUTTON, lambda e: end_dialog_safely(self, wx.ID_CANCEL))
        btn_sizer.Add(btn_cancel, 0)
        sizer.Add(btn_sizer, 0, wx.EXPAND | wx.ALL, 10)

        panel.SetSizer(sizer)
        self.Centre()
        self.text_prefix.SetFocus()

    def on_ok(self, event):
        prefix = str(self.text_prefix.GetValue() or "").strip()
        if not prefix:
            wx.MessageBox("名称前缀不能为空。", "输入错误", wx.OK | wx.ICON_WARNING, self)
            return
        start = int(self.spin_start.GetValue())
        if self.digit_choice_values:
            selected_index = int(self.choice_digits.GetSelection())
            if selected_index < 0 or selected_index >= len(self.digit_choice_values):
                wx.MessageBox("请选择编号格式。", "输入错误", wx.OK | wx.ICON_WARNING, self)
                return
            digits = int(self.digit_choice_values[selected_index][0])
        else:
            digits = int(self.spin_digits.GetValue())
        count = int(self.fixed_count) if self.fixed_count is not None else int(self.spin_count.GetValue())
        if count <= 0:
            wx.MessageBox("数量必须大于 0。", "输入错误", wx.OK | wx.ICON_WARNING, self)
            return
        self.result = {
            "prefix": prefix,
            "start": max(0, start),
            "digits": max(0, digits),
            "count": count
        }
        end_dialog_safely(self, wx.ID_OK)
