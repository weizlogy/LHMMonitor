# LHMMonitor

LibreHardwareMonitorのセンサーデータをRainmeterから直接取得するプラグインです。

## これは？

「PCの温度とかCPU使用率とかをデスクトップに常駐表示させたい」と思ったとき...

### RTSS

DesktopOverlayHost64.exeを起動させるだけ。だけどもタスクバーにいるの邪魔。

### Rainmeter

HWinfo経由
- インストールして起動させておかないといけない？
- 共有メモリはFreeだと12時間制限？？？

Open Hardware Monitor経由
- インストールして起動させておかないといけない？

Libra Hardware Monitor経由
- インストールして起動させておかないといけない？
- ローカルWebサーバーを起動させておかないといけない？

とにかくなにかインストールして起動させておかないといけない？？？

このプラグインは、LHMのライブラリ（LibreHardwareMonitorLib）をRainmeterプロセス内で直接ホストすることで
センサーデータを取得するため、ほかにインストールや常駐させる必要はありません。

## 特徴

- **ダイレクトアクセス**: LHMのライブラリを直接呼び出すため、HTTPやWMIのオーバーヘッドがありません
- **バックグラウンド更新**: 別スレッドで定期的にセンサーを更新するため、RainmeterのUpdate頻度を上げてもモッサリしません
- **柔軟な指定**: `HardwareName` / `SensorType` / `SensorName`の3パラメータで、目的のセンサーをピンポイントで指定
- **デバッグ機能**: `Debug=1`で、利用可能なセンサー一覧を画面に表示。HardwareNameやSensorNameの調査が簡単です

## 動作要件

- Rainmeter 4.x
- Windows 10 / 11
- **Rainmeterを管理者権限で起動**すること（LHMがハードウェアにアクセスするため）
- **.NET Framework 4.7.2** 以上

## 制限事項

- **LHM本体（GUI）およびLHM Data Providerは停止してください**。
  ハードウェアアクセス用のカーネルドライバー（PawnIo）を奪い合うため、同時に動作させるとデータ取得に失敗する可能性があります。
- センサー名（`HardwareName`、`SensorName`）は環境依存です。
  最初はDebugスキンで一覧を確認し、正確な名前をiniにコピーしてください。

## 開発環境

- Visual Studio
- LHMMonitorフォルダーと同階層にrainmeter-plugin-sdkの`API`フォルダーをコピーしてください。

## 簡単な使い方

- LHMMonitor.dllをRainmeterのPluginフォルダに入れます。（AppData\Roaming\Rainmeter\Plugins とか？）

- iniを書きます。

```ini
[MeasureCPUCoreTemp]
Measure=Plugin
Plugin=LHMMonitor.dll
HardwareName=AMD Ryzen 5 9600X
SensorType=Temperature
SensorName=Core (Tctl/Tdie)
```

- 動きます。
