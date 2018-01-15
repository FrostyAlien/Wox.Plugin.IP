using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using Newtonsoft.Json;
using System.Windows.Forms;
//using Wox.Plugin;

namespace Wox.Plugin.IP
{
    public class Main : IPlugin
    {
        private PluginInitContext _context;

        public List<Result> Query(Query query)
        {

            List<Result> results = new List<Result>();

            string error = "无互联网连接或远端服务器发生错误";
            string info = "请输入一段完整的IP地址";
            string BGPViewError = "数据库尚未更新这条前缀的信息。可能是因为这条前缀尚未广播或广播不久。";

            if (query.Search.Length == 0)
            {

                var client = new WebClient();
                client.Headers.Add("Host", "myip.ipip.net");

                string response = Encoding.GetEncoding("UTF-8").GetString(client.DownloadData("http://myip.ipip.net/"));

                //Try to match IP address. A shitty regex.
                Match match = Regex.Match(response, @"((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)");

                if (match.Success)
                {
                    client.Headers.Clear();
                    client.Headers.Add("Host", "freeapi.ipip.net");
                    client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:57.0) Gecko/20100101 Firefox/57.0");

                    string ipResponse = Encoding.GetEncoding("utf-8").GetString(client.DownloadData("https://freeapi.ipip.net/" + match.Value));

                    //Fix API data format
                    string converter = Regex.Replace(ipResponse, @"\[", "{\"IPIP\":[");
                    converter = Regex.Replace(converter, @"\]", "]}");

                    IPIPObject ipInfo = JsonConvert.DeserializeObject<IPIPObject>(converter);

                    client.Headers.Clear();
                    client.Headers.Add("Host", "api.bgpview.io");

                    string bgpResponse = Encoding.GetEncoding("utf-8").GetString(client.DownloadData("https://api.bgpview.io/ip/" + match.Value));
                    BGPViewObject bgpInfo = JsonConvert.DeserializeObject<BGPViewObject>(bgpResponse);

                    if (ipInfo.IPIP[0] != null)
                    {
                        var LocalIP = match.Value;
                        var IPDetails = ipInfo.IPIP[0] + " " + ipInfo.IPIP[1] + " " + ipInfo.IPIP[2] + " " + ipInfo.IPIP[4];

                        results.Add(new Result()
                        {
                            Title = LocalIP,
                            SubTitle = IPDetails,
                            IcoPath = "Images\\ipip.png",  //相对于插件目录的相对路径
                            Action = this.CopyToClipboardFunc(LocalIP + " " + IPDetails)
                        });
                    }

                    if (bgpInfo.Status.Equals("ok"))
                    {
                        var ASN = "AS" + bgpInfo.Data.Prefixes[0].Asn.PurpleAsn;
                        var minPerfix = bgpInfo.Data.Prefixes[0].PurplePrefix;
                        var PerfixesDetails = bgpInfo.Data.Prefixes[0].Name;
                        var ASNname = bgpInfo.Data.Prefixes[0].Asn.Name;

                        results.Add(new Result()
                        {
                            Title = ASN + " " + ASNname,
                            SubTitle = "Prefix: " + minPerfix + " " + PerfixesDetails,
                            IcoPath = "Images\\bgpview.png",  //相对于插件目录的相对路径
                            Action = this.CopyToClipboardFunc(ASN + " " + ASNname)
                        });
                    }

                    if (ipInfo.IPIP[0] == null && bgpInfo.Meta.ExecutionTime == null)
                    {
                        results.Add(new Result

                        {

                            Title = error

                        });
                    }

                }
                else
                {
                    results.Add(new Result

                    {

                        Title = "Match faild"

                    });
                }

            }
            else
            {
                Match match = Regex.Match(query.Search, @"((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)");

                if (match.Success)
                {
                    var client = new WebClient();
                    client.Headers.Add("Host", "freeapi.ipip.net");
                    client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:57.0) Gecko/20100101 Firefox/57.0");

                    string ipResponse = Encoding.GetEncoding("utf-8").GetString(client.DownloadData("https://freeapi.ipip.net/" + query.Search));

                    string converter = Regex.Replace(ipResponse, @"\[", "{\"IPIP\":[");
                    converter = Regex.Replace(converter, @"\]", "]}");

                    IPIPObject ipInfo = JsonConvert.DeserializeObject<IPIPObject>(converter);

                    client.Headers.Clear();
                    client.Headers.Add("Host", "api.bgpview.io");

                    string bgpResponse = Encoding.GetEncoding("utf-8").GetString(client.DownloadData("https://api.bgpview.io/ip/" + query.Search));
                    BGPViewObject bgpInfo = JsonConvert.DeserializeObject<BGPViewObject>(bgpResponse);

                    if (ipInfo.IPIP[0] != null)
                    {
                        var LocalIP = query.Search;
                        var IPDetails = ipInfo.IPIP[0] + " " + ipInfo.IPIP[1] + " " + ipInfo.IPIP[2] + " " + ipInfo.IPIP[4];

                        results.Add(new Result()
                        {
                            Title = LocalIP,
                            SubTitle = IPDetails,
                            IcoPath = "Images\\ipip.png",  //相对于插件目录的相对路径
                            Action = this.CopyToClipboardFunc(LocalIP + " " + IPDetails)
                        });
                    }

                    if (bgpInfo.Status.Equals("ok"))
                    {
                        string ASN,minPerfix,ASNname, PerfixesDetails;
                        ASN = null;
                        minPerfix = null;
                        ASNname = null;
                        PerfixesDetails = null;

                        try
                        {
                            ASN = "AS" + bgpInfo.Data.Prefixes[0].Asn.PurpleAsn;
                            minPerfix = bgpInfo.Data.Prefixes[0].PurplePrefix;
                            PerfixesDetails = bgpInfo.Data.Prefixes[0].Name;
                            ASNname = bgpInfo.Data.Prefixes[0].Asn.Name;

                        } catch {

                            results.Add(new Result

                            {

                                Title = BGPViewError

                            });

                        }
                        

                        results.Add(new Result()
                        {
                            Title = ASN + " " + ASNname,
                            SubTitle = "Prefix: " + minPerfix + " " + PerfixesDetails,
                            IcoPath = "Images\\bgpview.png",  //相对于插件目录的相对路径
                            Action = this.CopyToClipboardFunc(ASN + " " + ASNname)
                        });
                    }

                    if (ipInfo.IPIP[0] == null && bgpInfo.Meta.ExecutionTime == null)
                    {
                        results.Add(new Result

                        {

                            Title = error

                        });
                    }

                } else {

                    results.Add(new Result

                    {

                        Title = info

                    });
                }
            }
            return results;
        }

        private System.Func<ActionContext, bool> CopyToClipboardFunc(string text)
        {
            return c =>
            {
                if (this.CopyToClipboard(text))
                {
                    _context.API.ShowMsg("信息已被存入剪贴板");
                }
                else
                {
                    _context.API.ShowMsg("剪贴板打开失败，请稍后再试");
                }
                return false;
            };
        }

        private bool CopyToClipboard(string text)
        {
            try
            {
                Clipboard.SetText(text);
            }
            catch (System.Exception e)
            {
                return false;
            }
            return true;
        }
        // Start
        public void Init(PluginInitContext context)
        {
            _context = context;
        }
    }

    //IPIP Free API Data Structure
    public class IPIPObject
    {
        /*
         * 0 Country
         * 1 Province
         * 2 City
         * 3 Useless Value
         * 4 ISP
         */
        public IList<string> IPIP { get; set; }

    }


    /*
     * A Huge Data Structure From BGPView.io API. Some data are deleted.
     * Test URL for review: https://api.bgpview.io/ip/8.8.8.8
     * Doc: https://bgpview.docs.apiary.io/#reference/0/ip/view-ip-address-details
     */
    public partial class BGPViewObject
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("status_message")]
        public string StatusMessage { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }

        [JsonProperty("@meta")]
        public Meta Meta { get; set; }
    }

    public partial class Data
    {
        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("ptr_record")]
        public string PtrRecord { get; set; }

        [JsonProperty("prefixes")]
        public Prefix[] Prefixes { get; set; }

        [JsonProperty("rir_allocation")]
        public RirAllocation RirAllocation { get; set; }

        [JsonProperty("iana_assignment")]
        public IanaAssignment IanaAssignment { get; set; }

        [JsonProperty("maxmind")]
        public Maxmind Maxmind { get; set; }
    }

    public partial class IanaAssignment
    {
        [JsonProperty("assignment_status")]
        public string AssignmentStatus { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("whois_server")]
        public string WhoisServer { get; set; }

        [JsonProperty("date_assigned")]
        public object DateAssigned { get; set; }
    }

    public partial class Maxmind
    {
        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        [JsonProperty("city")]
        public object City { get; set; }
    }

    public partial class Prefix
    {
        [JsonProperty("prefix")]
        public string PurplePrefix { get; set; }

        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("cidr")]
        public long Cidr { get; set; }

        [JsonProperty("asn")]
        public Asn Asn { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("country_code")]
        public string CountryCode { get; set; }
    }

    public partial class Asn
    {
        [JsonProperty("asn")]
        public long PurpleAsn { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("country_code")]
        public string CountryCode { get; set; }
    }

    public partial class RirAllocation
    {
        [JsonProperty("rir_name")]
        public string RirName { get; set; }

        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("cidr")]
        public long Cidr { get; set; }

        [JsonProperty("prefix")]
        public string Prefix { get; set; }

        [JsonProperty("date_allocated")]
        public string DateAllocated { get; set; }

        [JsonProperty("allocation_status")]
        public string AllocationStatus { get; set; }
    }

    public partial class Meta
    {
        [JsonProperty("time_zone")]
        public string TimeZone { get; set; }

        [JsonProperty("api_version")]
        public long ApiVersion { get; set; }

        [JsonProperty("execution_time")]
        public string ExecutionTime { get; set; }
    }
}