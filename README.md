### 恶俗SB播放器
---
![](https://img.shields.io/github/v/release/MikiraSora/ReOsuStoryboardPlayer?label=ReOsuStoryboardPlayer)
<br>
![](https://img.shields.io/nuget/v/ReOsuStoryboardPlayer.Core?label=ReOsuStoryboardPlayer.Core)
<br>
![](https://img.shields.io/github/license/MikiraSora/ReOsuStoryboardPlayer)

#### 前言:
本软件基于本人魔改的另一个fork过来的2D渲染框架OpenGLF(SimpleRenderFramework),期初是为用于测试框架的物件动作动画系统的测试项目，现以独立。

#### 用法:
直接将osu铺面文件夹托给exe程序文件即可.或者使用[命令行](https://github.com/MikiraSora/OsuStoryBoardPlayer/wiki/Program-command-options).

#### 进展:
* 能够执行遵守基本法的SB以及大部分超级牛逼炫酷无敌的SB
* 正在完善LoopComand和其他命令的执行逻辑.
* 优化内存占用和渲染逻辑.
* TriggerCommand绝大部分实现
* 已实现SB转视频(支持高fps高分辨率的)
* 已实现实用的Debugger工具(至少不会对着屙屎那个界面脑内debug了)
* **核心逻辑独立成项目**，可方便移植到任何地方实现SB逻辑
* 提供WPF控件支持
* 提供Example项目

#### 已知问题
* 部分Sb铺面不能完全支持(可能有执行逻辑的bug，或者是sb作者写错代码)
* 部分老爷机不能支持部分铺面的执行(~~Vulkan在做了.gugugu~~)
* TriggerCommand可能执行有误(**但因为屙屎目前实现细节和以前不同**,比如TriggerGroup,所以可能有所差异)

#### 计划(咕)
* Profile工具
* ReStoryboardPlayer on Brosewer with Blazor

#### 系统环境要求
* .net framework 4.7.1

#### 截图:
![](https://puu.sh/FHdvD/6faa1025c8.png)
![](https://puu.sh/FHdEA/d6ca395899.png)
![](https://puu.sh/xku6E/3671305f79.jpg)
![](https://puu.sh/xkueL/72e434a5e7.png)
![](https://puu.sh/xkupr/51c48cc25a.png)
![](https://puu.sh/xkuxm/1bbd847777.png)
------
Oldvervion:
![](https://github.com/MikiraSora/OsuStoryBoardPlayer/blob/master/readme_img/1.png)
![](https://github.com/MikiraSora/OsuStoryBoardPlayer/blob/master/readme_img/2.png)
![](https://github.com/MikiraSora/OsuStoryBoardPlayer/blob/master/readme_img/3.png)
![](https://github.com/MikiraSora/OsuStoryBoardPlayer/blob/master/readme_img/4.png)

#### 引用项目
虽然绝大部分SB更新逻辑实现是独立实现的，但其他一些内容是使(偷)用(懒)使用到第三方库的:
* [storybrew](https://github.com/Damnae/storybrew) (MIT License)
* [osu-framework](https://github.com/ppy/osu-framework) (MIT License)
* [SaarFFmpeg](https://github.com/ibukisaar/SaarFFmpeg) (MIT License)
* 其他(比如Trigger部分实现)


#### Special Thanks
****超级牛逼无敌炫酷爆炸麻花****(43size),Damnae，鹅苗，外服大师 ,[kj415j45](https://github.com/kj415j45)，****[毛毛](https://github.com/KedamaOvO)****,peppy，量子玫瑰 and all of 恶俗麻婆****2857****吹比黄图群

#### 个人吐槽
**强烈呼吁Storyboard脚本检查进入osu!Rank图标准!!!!** <br>
**Strongly urge osu! Ranking Criteria contains Storyboard script checking**
