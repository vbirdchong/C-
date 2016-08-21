####使用C# 开发的项目

#####BarcodeMonitor

使用全局hook监听键盘的输入，将获得的输入数据发送给服务器进行数据库查询，并返回查询结果。通过所得的查询结果信息与叫号系统进行关联，调用API插入本地数据库。由于条码扫码枪的输入和键盘一样，因此不需额外驱动即可实现条码信息的获取。

##### BarcodeMonitorSetup

是BarcodeMonitor的安装发布程序，需要和叫号系统一起运行才可以。


##### IdConfirmApp

不需要额外扫描枪的ID测试程序

##### WpfApplication1

键盘全局hook使用的测试例子