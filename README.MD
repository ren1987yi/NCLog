# 基于FTOptix的 Node Controller Interface Demo项目

## 简介

项目包含两种读取方式

### 1. Remote Logging 

配置 : Web / NC Setting

![alt text](docs/image.png)



### 2. Web Interface
   读取日志文档

## 依赖

    - GWebApi : web api client 
    - RA.NodeControllerInterface : 信息处理库

## 项目介绍


![alt text](docs/image-2.png)

- NC_RemoteLog : Remote logging 画面
- NC_WebLog : 通过web api 获取log 信息
- MagneMotionRemoteloggingBehavior ： Remote log 的 Behavior ,需要实例化一个，对 remote log 进行处理
- MagneMotionLogMessageType : Log Message 的数据结构
  结构信息:
  ![alt text](docs/image-1.png)