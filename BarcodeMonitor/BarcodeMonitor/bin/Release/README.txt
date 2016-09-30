1.安装好排队管理系统后，在进行以下配置参数设置

2.相关配置参数设置
在安装好的目录下，找到BarcodeMonitor.exe.config文件后打开，将以配置项复制到该文件中，如果已经存在这些配置内容则
不需要再进行复制。请根据实际设备部署及业务需求进行修改配置内容
<?xml version="1.0"?>
<configuration>
  <appSettings>
    <!--叫号系统IP地址-->
    <add key="Network.Svc.Uri" value="net.tcp://127.0.0.1:81/" />
    <!--预约服务器地址-->
    <add key="Https.url" value="https://121.40.180.155:8082/V25/Vaccine/Index.aspx/ConfirmAppointInject" />
    <!--医院机构代码-->
    <add key="Authority" value="3301043301" />
    <!--医院机构名称-->
    <add key="AuthorityName" value="皋埠镇卫生院"/>
    <!--业务代码，需要和排队管理系统中的业务列表保持一致-->
    <!--ServiceCodeNoAppointment 未预约的业务代码-->
    <add key="ServiceCodeNoAppointment" value="A" />
    <!--ServiceCodeAppointment 预约的业务代码-->
    <add key="ServiceCodeAppointment" value="V"/>
  </appSettings>
</configuration>


配置参数说明：
a)
<!--叫号系统IP地址-->
<add key="Network.Svc.Uri" value="net.tcp://127.0.0.1:81/" />
该IP地址和 C:Program Files\Mmmer\Quere\Ticket\Mmmer.Queuer.Ticket.exe.config中的设置保持一致

b)
<!--预约服务器地址-->
<add key="Https.url" value="https://121.40.180.155:8082/V25/Vaccine/Index.aspx/ConfirmAppointInject" />
该URL由微象公司提供

c)
<!--医院机构代码-->
<add key="Authority" value="3301043301" />
医院机构代码统一由微象公司提供，每个机构拥有独立的机构代码

d)
<!--业务代码，需要和排队管理系统中的业务列表保持一致-->
在排队管理系统->业务管理->业务列表 一栏中，找到对应的业务名称和代码。
如：名称：未预约，代码：A
则设置为<add key="ServiceCodeNoAppointment" value="A" />
如：名称：已经预约，代码：V
则设置为<add key="ServiceCodeAppointment" value="V"/>


