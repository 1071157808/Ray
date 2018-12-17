# Ray
这是一个集成Actor,Event Sourcing(事件溯源),Eventual consistency(最终一致性)的无数据库事务，高性能分布式云框架(构建集群请参阅:http://dotnet.github.io/orleans/) 

### 环境搭建
1. 安装mongodb（或者PostgreSQL）,rabbitmq.
- 方式一：本机安装
- 方式二：使用Docker，调整对应参数，运行示例
```
docker run -d -p 5672:5672 -p 15672:15672 --hostname rabbit --name rabbit -e RABBITMQ_DEFAULT_USER=admin -e RABBITMQ_DEFAULT_PASS=admin  rabbitmq:3-management
docker run -d -p 27017:27017 --name mongo -td mongo 
docker run -d -p 5432:5432 --name postgres -e POSTGRES_PASSWORD=123456  postgres
```
### 案例说明

案例里是一个简单的无事务转账功能。

#### Example1

Example1是一个Console的示例。

1. 修改Host代码中的mongodb和rabbitmq的配置信息。

```csharp
    var builder = new SiloHostBuilder()
    .UseLocalhostClustering()
    .UseDashboard()
    .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
    .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(Account).Assembly).WithReferences())
    .ConfigureServices((context, servicecollection) =>
    {
        servicecollection.AddRay();
        servicecollection.AddSingleton<ISerializer, ProtobufSerializer>();//注册序列化组件
        //注册postgresql为事件存储库
       //servicecollection.AddPSqlSiloGrain();
        //注册mongodb为事件存储库
        servicecollection.AddMongoDbSiloGrain();
    })
    .Configure<SqlConfig>(c =>
    {
        c.ConnectionDict = new Dictionary<string, string> {
            { "core_event","Server=127.0.0.1;Port=5432;Database=Ray;User Id=postgres;Password=123456;Pooling=true;MaxPoolSize=20;"}
        };
    })
    .Configure<MongoConfig>(c =>
    {
        c.Connection = "mongodb://127.0.0.1:27017";
    })
    .Configure<RabbitConfig>(c =>
    {
        c.UserName = "admin";
        c.Password = "admin";
        c.Hosts = new[] { "127.0.0.1:5672" };
        c.MaxPoolSize = 100;
        c.VirtualHost = "/";
    })
    .ConfigureLogging(logging =>
    {
        logging.SetMinimumLevel(LogLevel.Error);
        logging.AddConsole();
    });
```

2. 修改client代码中的rabbitmq的配置信息。

```csharp
    client = await ClientFactory.Build(() =>
    {
        var builder = new ClientBuilder()
        .UseLocalhostClustering()
        .ConfigureServices((context, servicecollection) =>
        {
            servicecollection.AddRay();
            servicecollection.AddSingleton<ISerializer, ProtobufSerializer>();//注册序列化组件
            servicecollection.AddRabbitMQ();//注册RabbitMq为默认消息队列
            servicecollection.AddLogging(logging => logging.AddConsole());
            servicecollection.AddMQHandler();//注册所有handler
            servicecollection.PostConfigure<RabbitConfig>(c =>
            {
                c.UserName = "admin";
                c.Password = "admin";
                c.Hosts = new[] { "127.0.0.1:5672" };
                c.MaxPoolSize = 100;
                c.VirtualHost = "/";
            });
        })
        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IAccount).Assembly).WithReferences())
        .ConfigureLogging(logging => logging.AddConsole());
    });
```

3. 启动Ray.Host
4. 启动Ray.Client
5. 查看Orleans Dashboard，打开 http://localhost:8080
![img](https://note.youdao.com/yws/api/personal/file/F3763621DD114B678E16EB6F0A10A210?method=download&shareKey=50d0a0c58afe32247423c0cacefe8305)

#### Example2

Example2是一个Web的示例。有基于jwt的Token认证和转账功能。
1. 修改Server代码中的mongodb和rabbitmq的配置信息。
2. 修改WebApi代码中的rabbitmq的配置信息。
3. 启动Ray.Server
4. 启动Ray.WebApi

- API页面：http://localhost:59926/swagger/index.html
![img](https://note.youdao.com/yws/api/personal/file/4EB67A24A90D43EBAD31CBBCA2A2886B?method=download&shareKey=6bba648f418070c7cddca4d7dbf797b9)
- 获取Token，账号：Ray，密码是：123456
- Authorization
- 获得发送其他请求
注意：Bearer与Token之间有一个空格
![img](https://note.youdao.com/yws/api/personal/file/936358F9A50B44E584145A6F50055CE7?method=download&shareKey=2f3c0d8b901dc5a40d9dd60d2e4adecf)



#### 其他相关Example

[RayTale/OrleansSample](https://github.com/RayTale/OrleansSample)

这里面有两个Example
- AppClient/AppWebClient与AppHost是简单的Orleans的示例
- StatelessWebGo与StatelessHost是Service Fabric与Orleans结合的示例