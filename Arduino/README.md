# YMF825Board Arduino接続スケッチ

## 動作環境

- Arduino Nano

## シリアルポート接続仕様

- ボーレート: 256,000 bps
- パリティビット: なし

## コマンド仕様

| Command  | Name       | Parameter                                         | Return               |
|----------|------------|---------------------------------------------------|----------------------|
| **0x00** | Write      | Address, Data (total 2 bytes)                     | None                 |
| **0x01** | BurstWrite | Size (2 bytes), Address, Data (total 3 + n bytes) | None                 |
| **0x20** | Read       | Device, Address (total 2 bytes)                   | Data (1 byte)        |
| **0x40** | Select     | Device (1 byte)                                   | None                 |
| **0xFE** | HardReset  | None                                              | None                 |
| **0xFF** | Version    | None                                              | `V1YMF825` (8 bytes) |

パラメータは上表の順に行う。例えば、レジスタ `0x50` にデータ `0x42` を書き込む場合は `0x00 0x50 0x42` をシリアルポートに送信する。 

### Write コマンド

| Parameter | Size | Range           |
|-----------|-----:|-----------------|
| Address   | 1    | `0x00` - `0x7f` |
| Data      | 1    | `0x00` - `0xff` |

アドレスは最大 `0x7f` (127) であり、最上位ビットはマスクされる。

### BurstWrite コマンド

| Parameter | Size | Range                     |
|-----------|-----:|---------------------------|
| Size (LE) | 2    | `0x0000` - `0x0200` (512) |
| Address   | 1    | `0x00` - `0x7f`           |
| Data      | n    | `0x00` - `0xff`           |

パラメータ Size はパラメータ Data のバイト数。リトルエンディアン。512 バイトを超えた場合は 512 バイトの長さとして処理される。
アドレスは最大 `0x7f` (127) であり、最上位ビットはマスクされる。

### Read コマンド

| Parameter | Size | Range           |
|-----------|-----:|-----------------|
| * Device  | 1    | `0x00` - `0xf0` |
| Address   | 1    | `0x00` - `0x7f` |

単一のデバイスを読むため、Device パラメータのフラグは 1 つだけ `1` である必要がある。複数ある場合は最下位ビットから優先されて読み出される。
アドレスは最大 `0x7f` (127) であり、最上位ビットは自動的に `1` が立てられる。

### Select コマンド

| Parameter | Size | Range           |
|-----------|-----:|-----------------|
| * Device  | 1    | `0x00` - `0xff` |

複数のデバイスを指定できるため、Device パラメータの各フラグを 1 に指定可能。最大 8 デバイス。

### HardReset コマンド

YMF825Board の /RST ピンを Low レベルにし、ハードウェアリセットを行う。インタフェース側で必要な時間だけウェイトされる。

### Version コマンド

常に ASCII 文字の `V1YMF825` を返す。
