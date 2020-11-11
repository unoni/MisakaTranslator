# MisakaTranslator 御坂翻译器

本项目是 https://github.com/hanmin0822/MisakaTranslator 的fork。

功能增加：

* .NET5

功能缺失：

* TTS
* Mecab
* TesseractOCR
* J北京等32位库（如果用我构建的）
* 默认关闭人工翻译且未测试是否可用

构建：

```cmd
dotnet publish -c Release -p:RuntimeIdentifier=win10-x64 -p:PublishReadyToRun=true -p:DebugType=none --self-contained=false
```

结果在`MisakaTranslator\MisakaTranslator-WPF\bin\Release\net5.0-windows\win10-x64\publish`中。
