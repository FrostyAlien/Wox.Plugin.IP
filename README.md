# Wox.Plugin.IP
一个简易的Wox插件用于快速查找IP的位置信息和ASN信息。

![gif](https://imgur.com/7TjaUZA)

第一次写Wox的插件 代码比较乱 也不是很常用C#所以随便糊了一下 基本上是能用

API提供方：
IPIP.NET (freeapi.ipip.net)
BGPView.io (api.bgpview.io)

已知问题：
1. 由于使用的是IPIP.NET的免费API，在查询时有较大可能性超出每秒查询次数。主要体现在返回403 导致数据无法加载
2. 查询未广播或广播不久的IP段会有一个UI错误。处理中

此外本插件和Wox.Plugin.IP138的关键词冲突 可自行修改plugin.json解决
