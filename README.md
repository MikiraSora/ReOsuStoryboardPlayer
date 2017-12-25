### 恶俗SB播放器
---
#### 前言:
本软件基于本人魔改的另一个fork过来的2D渲染框架OpenGLF(SimpleRenderFramework),期初是为用于测试框架的物件动作动画系统的测试项目，现以独立。

#### 用法:
    直接将osu铺面文件夹托给exe程序文件即可.

#### 项目进展:
* 能够执行大部分守基本法的SB脚本,但真几把炫酷那种神SB暂时没能完全支持(
* SB物件的渲染层次问题**基本解决**
* 正在实现TriggerCommand/SampleObject/Variable/ColorRGBStr等
* 正在完善LoopComand和其他命令的执行逻辑.
* 优化内存占用和渲染逻辑.
* 从渲染框架独立,只依赖OpenTK和irrKlang.NET4
* 实现其他细节

#### 已知问题
* 部分Sb铺面不能完全支持
* 部分老爷机不能支持部分铺面的执行(固定铺面会跳出OpenGL内部异常)
* 部分使用RGB字符串和变量的谱面不会播放(因为还没实现)
* TriggerCommand会忽视(因为没实现)

#### 截图:
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


#### Special Thanks
****麻花****(43size),Damnae，鹅苗，外服大师 ,[kj415j45](https://github.com/kj415j45)，****[毛毛](https://github.com/KedamaOvO)****，量子玫瑰 and all of 恶俗麻婆****2857****吹比黄图群
