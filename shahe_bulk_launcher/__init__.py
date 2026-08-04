"""沙河批量打开器实现包。"""

__all__ = ("SandboxLauncher", "main")


def __getattr__(name):
    if name in __all__:
        from .app import SandboxLauncher, main

        return {"SandboxLauncher": SandboxLauncher, "main": main}[name]
    raise AttributeError(name)


def __dir__():
    return sorted(set(globals()) | set(__all__))
