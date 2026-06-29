# AI SuperResolution

### 说明:
> 本程序采用最新Avalonia+SukiUI作为图形界面, 使用原生C++重写NCNN-Vulkan图片处理引擎. 当前项目自带三种AI引擎, 能对模糊照片进行超分辨率放大处理, 开源无广告, `无需网络`即可免费使用.

#### 声明:
> 不得将该程序用于任何商业及违法用途

## 注意事项:
- 模型文件需要和当前exe文件在同一个目录下, 且`不可更改`模型文件夹名称, 否则无法正常运行, 下载之后, 保留原始的文件夹结构
- 软件处理完成的图片文件, 会在当前目录自动创建一个`output`文件夹, 所有输出的图片都放置在该目录

### 功能说明
- 1. 支持多线程处理
- 2. 支持批量图片处理
- 3. 支持设置选项
- 4. 支持自定义输出格式
- 5. 支持AI引擎选择
- 6. 支持批量清理任务
- 7. 自带AI引擎,无需网络即可使用

### 下载地址:
>[https://github.com/X-Lucifer/X.SuperResolution/releases](https://github.com/X-Lucifer/X.SuperResolution/releases)

### 系统要求:
  系统: Windows 10+
  运行时: .NET 8

### 运行截图
  ![](https://cdn.jsdelivr.net/gh/X-Lucifer/X.SuperResolution@main/docs/1.png)<br/>
  ![](https://cdn.jsdelivr.net/gh/X-Lucifer/X.SuperResolution@main/docs/2.png)<br/>
  ![](https://cdn.jsdelivr.net/gh/X-Lucifer/X.SuperResolution@main/docs/3.png)<br/>

### 第三方源码:
1. `realsr-ncnn-vulkan`: [https://github.com/nihui/realsr-ncnn-vulkan](https://github.com/nihui/realsr-ncnn-vulkan)
2. `srmd-ncnn-vulkan`: [https://github.com/nihui/srmd-ncnn-vulkan](https://github.com/nihui/srmd-ncnn-vulkan)
3. `waifu2x-ncnn-vulkan`: [https://github.com/nihui/waifu2x-ncnn-vulkan](https://github.com/nihui/waifu2x-ncnn-vulkan)
4. `SukiUI`: [https://github.com/nihui/srmd-ncnn-vulkan](https://github.com/kikipoulet/SukiUI)
