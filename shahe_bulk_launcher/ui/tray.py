import wx


class TaskBarIcon:
    def __init__(self, frame):
        self.frame = frame
        self.tb_icon = None
        try:
            import wx.adv
            self.tb_icon = wx.adv.TaskBarIcon()
            self.tb_icon.Bind(wx.adv.EVT_TASKBAR_LEFT_DOWN, self.on_left_click)
            self.tb_icon.Bind(wx.adv.EVT_TASKBAR_RIGHT_DOWN, self.on_right_click)
        except AttributeError:
            try:
                self.tb_icon = wx.TaskBarIcon()
                self.tb_icon.Bind(wx.EVT_TASKBAR_LEFT_DOWN, self.on_left_click)
                self.tb_icon.Bind(wx.EVT_TASKBAR_RIGHT_DOWN, self.on_right_click)
            except AttributeError:
                self.tb_icon = None
                print("当前系统不支持托盘图标功能")
        if self.tb_icon:
            self.set_icon()

    def set_icon(self):
        if not self.tb_icon:
            return
        icon = wx.ArtProvider.GetBitmap(wx.ART_INFORMATION, wx.ART_OTHER, (16, 16))
        self.tb_icon.SetIcon(wx.Icon(icon), "沙盒批量打开器")

    def on_left_click(self, event):
        if self.frame.IsIconized():
            self.frame.Iconize(False)
        if not self.frame.IsShown():
            self.frame.Show()
        self.frame.Raise()
        try:
            self.frame.lbl_status.SetLabel("已显示主窗口")
        except Exception:
            pass
        try:
            self.frame._speak_process_assist("已显示主窗口", event_key="launch_done")
        except Exception:
            pass

    def on_right_click(self, event):
        menu = wx.Menu()
        show_item = menu.Append(wx.ID_ANY, "显示主界面", "显示主界面")
        self.tb_icon.Bind(wx.EVT_MENU, self.on_show_window, show_item)
        menu.AppendSeparator()
        exit_item = menu.Append(wx.ID_EXIT, "退出", "退出程序")
        self.tb_icon.Bind(wx.EVT_MENU, self.on_exit, exit_item)
        self.tb_icon.PopupMenu(menu)
        menu.Destroy()

    def on_show_window(self, event):
        if self.frame.IsIconized():
            self.frame.Iconize(False)
        if not self.frame.IsShown():
            self.frame.Show()
        self.frame.Raise()
        try:
            self.frame.lbl_status.SetLabel("已显示主窗口")
        except Exception:
            pass
        try:
            self.frame._speak_process_assist("已显示主窗口", event_key="launch_done")
        except Exception:
            pass

    def on_exit(self, event):
        self.RemoveIcon()
        try:
            self.frame.force_exit_requested = True
        except Exception:
            pass
        self.frame.Close()

    def RemoveIcon(self):
        if self.tb_icon:
            try:
                self.tb_icon.RemoveIcon()
                self.tb_icon.Destroy()
            except:
                pass
            self.tb_icon = None

    def Destroy(self):
        self.RemoveIcon()
