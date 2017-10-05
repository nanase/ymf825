Ymf825
======

## 概要

C# から [libMPSSE](http://www.ftdichip.com/Support/Documents/AppNotes/AN_178_User%20Guide%20for%20LibMPSSE-SPI.pdf) を介して [YMF825Board](https://yamaha-webmusic.github.io/ymf825board/intro/) と接続するためのライブラリラッパと各種ライブラリです。
libMPSSE を使うにあたり、USB-SPI変換インタフェースとして [Adafruit FT232H Breakout](https://learn.adafruit.com/adafruit-ft232h-breakout/) を想定しています。

## プロジェクト

### Ymf825

libMPSSE の SPI ライブラリラッパと C# 向けの YMF825 ドライバクラスです。 

x86/x64向けの `libMPSSE.dll` を同梱しています。実行環境に応じて、読み込まれる DLL が自動で切り替わります。

### Ymf825Server

![サーバ動作画面](https://raw.githubusercontent.com/nanase/ymf825/master/doc/server.png)

YMF825Board にデータを転送するためのサーバです。接続先USBデバイスの選択、統計情報、YMF825Board に書き込まれたレジスタデータのマップが表示されます。

WCF を用いた IPC（プロセス間通信）でクライアントからのレジスタ制御命令を受け付けます。そのため、サーバは多重起動ができません。

## 動作確認環境

- Windows 10 64bit
- Adafruit FT232H Breakout
- YMF825Board

## 想定回路

![回路図](https://raw.githubusercontent.com/nanase/ymf825/master/doc/ymf825board.png)|![ブレッドボード写真 正面](https://raw.githubusercontent.com/nanase/ymf825/master/doc/breadboard_1.jpg)|![ブレッドボード写真 裏面](https://raw.githubusercontent.com/nanase/ymf825/master/doc/breadboard_2.jpg)|![ブレッドボード写真 上 配線](https://raw.githubusercontent.com/nanase/ymf825/master/doc/breadboard_3.jpg)
---|---|---|---
**回路図** | **正面** | **裏面** | **上**

FT232H と YMF825Board の接続は以下のとおりです。

### Adafruit FT232H Breakout

- **D0** : SCK
- **D1** : MOSI
- **D2** : MISO
- **D7** : /SS
  - 接続時に D7 を CS (/SS) とするよう変更しています。
- **C0** : /RST

### YMF825Board

- **/SS** : D7, LED -- L (GND) で点灯
- **MOSI** : D1
- **MISO** : D2
- **SCK** : D0
- **GND** : GND
- **5V** : 5V
- **/RST** : C0
- **Audio** : No connection
- **3.3V** : No connection


## ライセンス

### Ymf825

** [MIT License](./LICENSE) **

### Silk icon set 1.3

Ymf825Server のボタンとして使用しています。

This work is licensed under a
Creative Commons Attribution 2.5 License.
[ http://creativecommons.org/licenses/by/2.5/ ]


## リポジトリ作成者

七瀬 (Tomona Nanase)

[@nanase_coder](https://twitter.com/nanase_coder)
[nanase.cc](https://nanase.cc/)
