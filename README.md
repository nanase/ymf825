YMF825
======

## 概要

C# から [YMF825Board](https://yamaha-webmusic.github.io/ymf825board/intro/) に接続するためのライブラリラッパと各種ライブラリです。

## プロジェクト

### YMF825

C# 向けの YMF825 ドライバクラスです。FT232HL を介した SPI 接続や、YMF825 のための高レベル API を提供しています。 

### YMF825 Server

![サーバ動作画面](https://qiita-image-store.s3.amazonaws.com/0/30370/2ea26b8a-352c-f681-0c42-777264fd53e6.png)

接続先 USB デバイスの選択、統計情報、YMF825Board に書き込まれたレジスタデータのマップが表示されます。改良予定。

### YMF825 MIDI Driver

![トーンエディタ](https://qiita-image-store.s3.amazonaws.com/0/30370/f8f17244-a1eb-6a88-2a2f-290cc54eb8ce.png)

![イコライザエディタ](https://qiita-image-store.s3.amazonaws.com/0/30370/9ddb9ae0-7cb9-b50f-ed92-2d32bb20a27e.png)

MIDI メッセージを YMF825 のメッセージに変換します。GUI としてトーンエディタとイコライザエディタを搭載しています。
対応 MIDI メッセージは以下のとおりです。

| MIDI イベント | hex | 備考 |
|---|---|---|
| NoteOff | `80` | ノートオフベロシティは対応なし |
| NoteOn  | `90` |  |
| ControlChange | `b0` | 下表参照 |
| ProgramChange | `c0` | バンク未対応 |
| Pitchbend | `e0` |  |
| SystemExclusiveF0 | `f0` | 下表参照 |
| SystemExclusiveF7 | `f7` | 下表参照 |

| SystemExclusive メッセージ | hex | 備考 |
|---|---|---|
| ドラムパート変更 | `41 10 42 12 40 xx 15 yy` | `yy` は `0` (melody) とそれ以外で判別 |

| ControlChange | dec | 備考 |
|---|---|---|
| DataEntry MSB | `6` |  |
| Volume        | `7` | 初期値 `100` |
| Panpot        | `10` | 初期値 `64` (中央) |
| Expression    | `11` | 初期値 `127` |
| DataEntry LSB | `36` |  |
| RPN LSB | `100` | 下表参照 |
| RPN MSB | `101` | 下表参照 |
| Equalizer | `112` | イコライザ番号指定のための独自コントロール |

| RPN | MSB | LSB | 備考 |
|---|---|---|---|
| Pitchbend Sensitivity | `00` | `00` | 初期値 `2` |
| Master Finetune | `00` | `01` |  |

## 動作確認環境

- Windows 10 64bit
- CBW-YMF825-BB rev0.1

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
