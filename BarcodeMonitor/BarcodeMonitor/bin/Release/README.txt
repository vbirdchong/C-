1.��װ���Ŷӹ���ϵͳ���ڽ����������ò�������

2.������ò�������
�ڰ�װ�õ�Ŀ¼�£��ҵ�BarcodeMonitor.exe.config�ļ���򿪣�����������Ƶ����ļ��У�����Ѿ�������Щ����������
����Ҫ�ٽ��и��ơ������ʵ���豸����ҵ����������޸���������
<?xml version="1.0"?>
<configuration>
  <appSettings>
    <!--�к�ϵͳIP��ַ-->
    <add key="Network.Svc.Uri" value="net.tcp://127.0.0.1:81/" />
    <!--ԤԼ��������ַ-->
    <add key="Https.url" value="https://121.40.180.155:8082/V25/Vaccine/Index.aspx/ConfirmAppointInject" />
    <!--ҽԺ��������-->
    <add key="Authority" value="3301043301" />
    <!--ҽԺ��������-->
    <add key="AuthorityName" value="�޲�������Ժ"/>
    <!--ҵ����룬��Ҫ���Ŷӹ���ϵͳ�е�ҵ���б���һ��-->
    <!--ServiceCodeNoAppointment δԤԼ��ҵ�����-->
    <add key="ServiceCodeNoAppointment" value="A" />
    <!--ServiceCodeAppointment ԤԼ��ҵ�����-->
    <add key="ServiceCodeAppointment" value="V"/>
  </appSettings>
</configuration>


���ò���˵����
a)
<!--�к�ϵͳIP��ַ-->
<add key="Network.Svc.Uri" value="net.tcp://127.0.0.1:81/" />
��IP��ַ�� C:Program Files\Mmmer\Quere\Ticket\Mmmer.Queuer.Ticket.exe.config�е����ñ���һ��

b)
<!--ԤԼ��������ַ-->
<add key="Https.url" value="https://121.40.180.155:8082/V25/Vaccine/Index.aspx/ConfirmAppointInject" />
��URL��΢��˾�ṩ

c)
<!--ҽԺ��������-->
<add key="Authority" value="3301043301" />
ҽԺ��������ͳһ��΢��˾�ṩ��ÿ������ӵ�ж����Ļ�������

d)
<!--ҵ����룬��Ҫ���Ŷӹ���ϵͳ�е�ҵ���б���һ��-->
���Ŷӹ���ϵͳ->ҵ�����->ҵ���б� һ���У��ҵ���Ӧ��ҵ�����ƺʹ��롣
�磺���ƣ�δԤԼ�����룺A
������Ϊ<add key="ServiceCodeNoAppointment" value="A" />
�磺���ƣ��Ѿ�ԤԼ�����룺V
������Ϊ<add key="ServiceCodeAppointment" value="V"/>


