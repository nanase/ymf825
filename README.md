Ymf825
======

## 概要

C# から Arduino Nano を介して [YMF825Board](https://yamaha-webmusic.github.io/ymf825board/intro/) と接続するためのライブラリラッパと各種ライブラリです。

## プロジェクト

### Ymf825

Arduino Nano 接続ライブラリと C# 向けの YMF825 ドライバクラスです。 

### Ymf825Server

![サーバ動作画面](https://raw.githubusercontent.com/nanase/ymf825/master/doc/server.png)

YMF825Board にデータを転送するためのサーバです。接続先USBデバイスの選択、統計情報、YMF825Board に書き込まれたレジスタデータのマップが表示されます。

WCF を用いた IPC（プロセス間通信）でクライアントからのレジスタ制御命令を受け付けます。そのため、サーバは多重起動ができません。

## 動作確認環境

- Windows 10 64bit
- Arduino Nano
- YMF825Board (2個)

## 想定回路

![回路図](https://raw.githubusercontent.com/nanase/ymf825/master/doc/ymf825board.png)|![ブレッドボード写真 正面](https://raw.githubusercontent.com/nanase/ymf825/master/doc/breadboard_1.jpg)|![ブレッドボード写真 裏面](https://raw.githubusercontent.com/nanase/ymf825/master/doc/breadboard_2.jpg)|![ブレッドボード写真 上 配線](https://raw.githubusercontent.com/nanase/ymf825/master/doc/breadboard_3.jpg)
:-:|:-:|:-:|:-:
**回路図** | **正面** | **裏面** | **上**

## ライセンス

### Ymf825

[** MIT License **](./LICENSE)

### Silk icon set 1.3

Ymf825Server のボタンとして使用しています。

```
This work is licensed under a
Creative Commons Attribution 2.5 License.
[ http://creativecommons.org/licenses/by/2.5/ ]
```

## リポジトリ作成者

七瀬 (Tomona Nanase)

- [@nanase_coder](https://twitter.com/nanase_coder)
- [nanase.cc](https://nanase.cc/)
