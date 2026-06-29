# X.SuperResolution 文件夹架构说明

> 本文档描述 X.SuperResolution 发行版根目录下的所有文件和文件夹及其用途。

---

## 根目录

```
X.SuperResolution/
├── X.SuperResolution.exe          			# 主程序可执行文件
├── models/                        			# Real-ESRGAN 模型
├── models-cunet/                  			# CUNet 模型（waifu2x 经典架构）
├── models-srmd/                   			# SRMD 模型（超分辨率+噪声处理）
├── models-upconv_7_anime_style_art_rgb/  	# Upconv7 动漫风格模型
├── models-upconv_7_photo/         			# Upconv7 照片模型
└── output           						# 输出文件夹
```

---

## 1. `models/` — Real-ESRGAN 模型

Real-ESRGAN（Real-World ESRGAN）用于真实世界图像的超分辨率重建，尤其擅长处理未知退化的图像。

| 文件 | 说明 |
|------|------|
| `realesrgan-x4plus.bin` / `.param` | **通用 4× 放大模型**，适用于照片、风景等真实场景 |
| `realesrgan-x4plus-anime.bin` / `.param` | **动漫专用 4× 放大模型**，针对动漫/二次元图像优化 |
| `realesr-animevideov3-x2.bin` / `.param` | **动漫视频 v3 2× 放大模型** |
| `realesr-animevideov3-x3.bin` / `.param` | **动漫视频 v3 3× 放大模型** |
| `realesr-animevideov3-x4.bin` / `.param` | **动漫视频 v3 4× 放大模型** |

---

## 2. `models-cunet/` — CUNet 模型

CUNet 是 waifu2x 经典卷积神经网络架构，支持降噪和放大两个功能。

### 纯降噪模型（不放大）

| 文件 | 说明 |
|------|------|
| `noise0_model.bin` / `.param` | 降噪等级 **0**（轻微降噪） |
| `noise1_model.bin` / `.param` | 降噪等级 **1**（中等降噪） |
| `noise2_model.bin` / `.param` | 降噪等级 **2**（较强降噪） |
| `noise3_model.bin` / `.param` | 降噪等级 **3**（强力降噪） |

### 纯放大模型（不降噪）

| 文件 | 说明 |
|------|------|
| `scale2.0x_model.bin` / `.param` | **2× 放大**，无降噪 |

### 降噪 + 放大组合模型

| 文件 | 说明 |
|------|------|
| `noise0_scale2.0x_model.bin` / `.param` | 降噪 0 级 + 2× 放大 |
| `noise1_scale2.0x_model.bin` / `.param` | 降噪 1 级 + 2× 放大 |
| `noise2_scale2.0x_model.bin` / `.param` | 降噪 2 级 + 2× 放大 |
| `noise3_scale2.0x_model.bin` / `.param` | 降噪 3 级 + 2× 放大 |

---

## 3. `models-srmd/` — SRMD 模型

SRMD（Super-Resolution with Multiple Degradations）支持不同放大倍率和噪声退化处理。

### 带噪声退化

| 文件 | 说明 |
|------|------|
| `srmdnf_x2.bin` / `.param` | **SRMDNF 2× 放大**（Noise-Free 无噪声） |
| `srmdnf_x3.bin` / `.param` | **SRMDNF 3× 放大** |
| `srmdnf_x4.bin` / `.param` | **SRMDNF 4× 放大** |

### 基础 SRMD

| 文件 | 说明 |
|------|------|
| `srmd_x2.bin` / `.param` | **SRMD 2× 放大** |
| `srmd_x3.bin` / `.param` | **SRMD 3× 放大** |
| `srmd_x4.bin` / `.param` | **SRMD 4× 放大** |

---

## 4. `models-upconv_7_anime_style_art_rgb/` — Upconv7 动漫风格模型

基于 Upconv7 架构，针对动漫/二次元风格艺术图像优化，仅支持 2× 放大。

| 文件 | 说明 |
|------|------|
| `scale2.0x_model.bin` / `.param` | 纯 2× 放大（不降噪） |
| `noise0_scale2.0x_model.bin` / `.param` | 降噪 0 级 + 2× 放大 |
| `noise1_scale2.0x_model.bin` / `.param` | 降噪 1 级 + 2× 放大 |
| `noise2_scale2.0x_model.bin` / `.param` | 降噪 2 级 + 2× 放大 |
| `noise3_scale2.0x_model.bin` / `.param` | 降噪 3 级 + 2× 放大 |

---

## 5. `models-upconv_7_photo/` — Upconv7 照片模型

基于 Upconv7 架构，针对真实照片优化，仅支持 2× 放大。

| 文件 | 说明 |
|------|------|
| `scale2.0x_model.bin` / `.param` | 纯 2× 放大（不降噪） |
| `noise0_scale2.0x_model.bin` / `.param` | 降噪 0 级 + 2× 放大 |
| `noise1_scale2.0x_model.bin` / `.param` | 降噪 1 级 + 2× 放大 |
| `noise2_scale2.0x_model.bin` / `.param` | 降噪 2 级 + 2× 放大 |
| `noise3_scale2.0x_model.bin` / `.param` | 降噪 3 级 + 2× 放大 |

---

## 文件格式说明

- **`.bin`**：模型权重二进制文件，包含神经网络训练后的参数
- **`.param`**：模型结构描述文件，定义网络的层结构、超参数等
- **`.exe`**：Windows 可执行主程序

> `.bin` 和 `.param` 是 **NCNN** 推理框架的标准模型格式，两者必须成对使用。

---

## 模型架构对比

| 架构 | 适用场景 | 放大倍数 | 降噪支持 |
|------|----------|----------|----------|
| **Real-ESRGAN** | 真实世界照片/动漫视频 | 2×/3×/4× | 内置 |
| **CUNet** | 经典 waifu2x，兼容性好 | 1×/2× | 0~3 级可调 |
| **SRMD** | 多重退化处理 | 2×/3×/4× | NF/带噪声 |
| **Upconv7 Anime** | 动漫/二次元艺术图 | 2× | 0~3 级可调 |
| **Upconv7 Photo** | 真实照片 | 2× | 0~3 级可调 |
