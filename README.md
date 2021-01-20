# MisakaTranslator 御坂翻译器

本项目是 https://github.com/hanmin0822/MisakaTranslator 的fork。

功能增加：

* .NET5
* x64

功能缺失：

* TTS
* Mecab
* TesseractOCR
* J北京等32位库
* 默认关闭人工翻译且未测试是否可用
* 全屏时原来能显示的现在也许不能显示

没有解决：

* 反复打开设置界面会导致内存不断增长
* 最小翻译间隔（防抖）
* 有道不可用

构建（需要装SDK）：

```cmd
dotnet publish -c Release -r win10-x64 -p:PublishReadyToRun=true -p:DebugType=none --self-contained=false
```

结果在`MisakaTranslator\MisakaTranslator-WPF\bin\Release\net5.0-windows\win10-x64\publish`中。

你也可以下载我release的包，但不保证为最新的；运行必须装好.NET Desktop Runtime（SDK中已包含）。
