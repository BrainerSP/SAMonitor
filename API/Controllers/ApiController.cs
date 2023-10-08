﻿using Dapper;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using SAMonitor.Data;
using SAMonitor.Utils;

namespace SAMonitor.Controllers;

[ApiController]
[Route("api")]
public class ApiController : ControllerBase
{
    private readonly ILogger<ApiController> _logger;

    public ApiController(ILogger<ApiController> logger)
    {
        _logger = logger;
    }

    [HttpGet("GetServerByIP")]
    public Server? GetServerByIP(string ip_addr)
    {
        ServerManager.ApiHits++;
        Server? result = ServerManager.ServerByIP(ip_addr);

        return result;
    }

    [HttpGet("GetAllServers")]
    public IEnumerable<Server> GetAllServers()
    {
        ServerManager.ApiHits++;

        return ServerManager.GetServers();
    }

    [HttpGet("GetFilteredServers")]
    public List<Server> GetFilteredServers(int show_empty = 0, string order = "none", string name = "unspecified", string gamemode = "unspecified", int hide_roleplay = 0, int paging_size = 0, int page = 0, string version = "any", string language = "any", int require_sampcac = 0, int show_passworded = 0, int only_openmp = 0)
    {
        ServerManager.ApiHits++;

        ServerFilterer filterServers = new(
                showEmpty: show_empty != 0,
                showPassworded: show_passworded != 0,
                hideRoleplay: hide_roleplay != 0,
                requireSampCAC: require_sampcac != 0,
                onlyOpenMp: only_openmp != 0,
                order: order,
                name: name.ToLower(),
                gamemode: gamemode.ToLower(),
                language: language.ToLower(),
                version: version.ToLower()
            );

        List<Server> orderedServers = filterServers.GetFilteredServers();

        // If we're paging, return a "page".
        if (paging_size > 0)
        {
            try
            {
                return orderedServers.Skip(paging_size * page).Take(paging_size).ToList();
            }
            catch
            {
                // nothing, just return the full list. this just protects from malicious pagingSize or page values.
            }
        }

        return orderedServers;
    }

    [HttpGet("GetServerPlayers")]
    public List<Player> GetServerPlayers(string ip_addr)
    {
        ServerManager.ApiHits++;

        var result = ServerManager.ServerByIP(ip_addr);

        if (result is null) return new List<Player>();

        return result.GetPlayers();
    }

    [HttpGet("GetGlobalStats")]
    public GlobalStats GetGlobalStats()
    {
        return
            new GlobalStats(
                serversOnline: ServerManager.ServerCount(),
                serversTracked: ServerManager.ServerCount(includeDead: true),
                serversOnlineOMP: ServerManager.ServerCount(onlyOMP: true),
                playersOnline: ServerManager.TotalPlayers()
            );
    }

    [HttpGet("GetLanguageStats")]
    public LanguageStats GetLanguageStats()
    {
        return ServerManager.LanguageAnalytics();
    }

    [HttpGet("GetGamemodeStats")]
    public GamemodeStats GetGamemodeStats()
    {
        return ServerManager.GamemodeAnalytics();
    }

    [HttpGet("GetMasterlist")]
    public string GetMasterlist(string version = "any")
    {
        ServerManager.ApiHits++;

        return ServerManager.GetMasterlist(version);
    }

    private long lastAddReq = 0;

    [HttpGet("AddServer")]
    public async Task<string> AddServer(string ip_addr)
    {
        if (lastAddReq >= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        {
            return "Please try again in a few seconds.";
        }

        lastAddReq = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3;

        ServerManager.ApiHits++;

        ip_addr = ip_addr.Trim();
        string validIP = Helpers.ValidateIPv4(ip_addr);
        if (validIP != "invalid")
            return (await ServerManager.AddServer(validIP));
        else
            return "Entered IP address or hostname is invalid or failing to resolve.";
    }

    [HttpGet("fuckshitup")]
    public async Task<bool> FuckShitUp()
    {
        string ip_text = "51.254.84.40:1441;91.121.87.14:3899;54.38.117.77:1269;54.38.117.77:1153;164.132.219.35:7771;91.121.87.14:9546;54.36.82.238:1769;91.121.87.14:6061;51.75.232.69:2673;5.39.108.48:1437;54.38.117.72:1733;178.32.226.173:7777;54.36.82.238:1841;91.121.87.14:9313;91.121.87.14:5369;91.121.87.14:4822;54.38.117.75:7777;91.134.166.78:7777;5.9.8.124:14281;23.88.73.88:13966;178.33.32.52:7777;23.88.73.88:18332;5.9.8.124:17918;23.88.73.88:34393;5.9.8.124:13350;158.101.170.155:7777;185.222.242.180:7777;46.174.49.47:7811;217.106.106.85:7018;31.186.250.169:1111;82.165.79.161:7777;51.68.175.3:7013;62.171.171.97:8500;141.94.184.106:7777;91.134.166.77:4444;91.121.237.130:9999;5.9.8.124:17897;23.88.73.88:14024;87.229.6.238:9999;144.76.57.59:14972;144.76.57.59:19068;144.76.57.59:19063;144.76.57.59:14556;144.76.57.59:34855;37.221.94.25:7777;185.213.174.163:4739;135.125.112.188:7777;213.32.120.116:7777;51.91.91.98:7777;188.165.59.228:7777;188.165.59.228:9999;185.213.174.162:5729;141.94.184.108:1189;164.132.219.35:4057;65.108.101.89:30740;65.108.101.89:30406;65.108.101.89:30154;65.108.101.89:31720;51.83.147.128:7777;185.189.15.89:7002;185.189.15.89:7066;45.125.66.177:9955;217.106.106.85:7009;46.174.50.20:7791;217.106.106.47:7777;23.88.73.88:9637;5.9.8.124:17379;23.88.73.88:13874;193.17.92.160:1208;185.189.15.22:2270;46.183.184.33:5217;46.183.184.33:5830;46.183.184.33:5676;185.189.15.24:6338;149.56.41.54:7772;149.56.41.51:7780;149.56.41.50:7780;149.56.41.52:7786;15.204.244.59:7777;149.56.41.51:7772;51.81.57.217:28561;144.217.174.215:7779;51.81.57.217:27740;104.167.222.158:6999;149.56.41.49:7770;37.230.162.160:7777;188.212.102.118:7777;188.212.100.126:7777;45.125.66.177:7771;51.81.166.66:24158;189.207.21.176:8787;189.207.21.176:9999;189.207.21.176:8888;143.42.70.139:7777;172.104.169.31:7777;170.187.227.75:2145;212.46.36.107:7777;189.127.164.37:2009;51.79.218.114:7777;51.79.197.132:5555;51.81.166.66:24623;51.79.181.126:7753;51.79.218.114:9999;82.65.202.73:7777;188.166.249.114:7775;34.93.197.30:7777;5.252.35.3:7777;82.208.16.35:7777;141.95.3.158:7777;78.31.65.74:2023;103.164.54.250:7777;194.233.174.189:7777;162.19.241.73:7777;178.32.226.173:6666;185.222.242.180:7788;77.68.118.192:6969;167.172.93.175:7777;193.203.39.47:7777;193.203.39.46:7777;185.222.242.180:7772;149.202.42.25:7777;185.201.113.14:7777;178.33.35.183:7777;151.80.19.151:7777;185.137.94.38:7777;141.11.230.139:7777;51.77.48.144:7777;51.38.115.171:7777;62.122.213.60:7777;82.208.16.35:7778;212.24.103.83:7777;193.17.92.160:1197;185.139.70.129:1123;46.105.169.128:7777;188.225.27.18:7777;212.118.36.220:7777;62.122.214.196:7777;87.229.6.238:7777;5.63.155.149:7777;37.187.77.206:7777;91.134.244.215:7777;217.144.54.222:7093;51.178.185.229:7777;62.171.171.97:7782;149.202.39.188:7777;185.185.69.95:7777;51.79.84.142:7777;149.56.45.135:7777;135.148.104.146:7777;147.135.91.184:7777;158.69.123.27:7777;37.230.228.164:7777;54.39.97.233:7777;51.79.71.138:7777;15.204.209.239:7777;45.56.97.129:7777;137.184.194.17:7777;172.105.118.76:7777;191.101.234.67:7777;167.172.93.175:7703;167.172.93.175:7701;188.166.249.114:7777;167.172.93.175:7702;167.172.93.175:7704;103.188.82.195:7777;185.169.134.45:7777;217.160.170.209:7777;185.169.134.83:7777;149.18.63.140:7777;195.90.208.44:7777;46.105.43.212:8000;141.94.184.111:3417;91.134.166.78:6666;188.165.45.72:7777;213.32.120.115:7777;91.121.237.131:7777;144.76.57.59:10228;141.145.217.244:7778;91.134.166.79:7777;54.38.201.18:5555;51.83.153.240:7777;94.23.168.153:1279;141.95.190.148:7777;185.213.174.139:5222;51.255.203.71:7777;91.121.87.14:8234;135.125.182.79:7777;213.32.120.190:7777;178.32.60.120:8800;51.254.130.14:7777;91.121.237.128:7778;5.39.108.50:1279;185.169.134.164:7777;135.125.234.229:7777;163.172.179.104:7777;185.169.134.166:7777;5.39.108.55:1121;51.91.91.91:7777;195.201.142.254:7777;37.48.87.211:7778;79.137.97.50:7777;51.91.91.65:7777;91.134.193.97:7850;138.3.253.104:7777;37.120.189.179:7777;116.202.237.220:7777;81.0.217.177:27969;51.83.217.86:7777;141.95.234.16:1161;54.36.188.222:7777;31.220.43.211:7777;185.169.134.62:8904;95.216.54.187:7720;89.41.176.63:7777;54.93.213.88:7777;176.32.39.166:7777;51.91.91.111:7777;51.89.188.190:7777;51.91.91.76:7777;45.125.66.27:4000;185.169.134.3:7777;176.32.39.5:7777;141.94.184.105:1117;185.169.134.60:8904;46.105.43.212:8888;188.214.88.80:7777;141.145.217.244:7777;81.4.103.173:7777;164.132.200.55:1259;176.32.36.95:7777;91.121.87.14:7408;51.254.178.238:7780;185.169.134.107:7777;54.38.103.220:1154;82.208.17.10:27988;94.23.168.153:2443;54.38.103.220:1114;51.91.91.97:7777;79.137.97.42:7777;54.38.103.220:1162;5.135.24.181:7779;82.208.16.224:7778;176.31.198.241:7777;91.121.87.14:2572;178.63.13.186:15725;176.32.39.132:7777;51.83.217.86:7783;45.125.66.151:5656;5.196.112.104:7729;51.89.188.191:7777;137.74.4.4:7777;141.94.184.105:1423;86.57.202.49:7777;188.212.100.119:7777;91.121.237.130:7777;51.91.91.113:7777;91.121.87.14:8101;81.180.203.55:7777;185.53.130.171:1337;164.132.200.55:1547;152.228.156.128:7777;46.105.43.212:7777;185.34.216.66:7777;195.181.247.190:7777;185.169.134.173:7777;185.169.134.44:7777;45.125.66.97:7760;194.163.189.165:7777;46.39.225.193:7780;193.203.39.218:7777;164.132.200.55:1899;176.32.39.56:7777;149.202.88.189:7777;91.121.43.38:7777;188.212.100.127:7777;178.32.98.14:7777;46.174.50.45:7845;51.210.180.137:7777;176.32.36.87:7777;188.212.102.123:7777;176.32.36.122:7777;54.38.117.76:1205;91.211.118.49:7777;91.211.118.21:7780;176.32.36.144:7777;51.91.91.89:7777;54.38.117.76:1353;151.80.47.21:7777;46.174.50.20:7782;141.94.184.109:7777;185.169.134.171:7777;202.61.198.12:7777;176.32.36.4:5555;198.251.81.224:7777;54.36.235.150:7777;185.169.134.4:7777;91.134.166.79:8888;185.169.134.61:7777;37.230.210.152:7777;145.239.136.61:6666;46.174.51.182:7776;46.174.54.87:7777;176.32.39.26:7777;49.12.12.240:7777;80.66.82.242:7777;176.32.39.22:7777;176.32.39.40:7777;194.93.2.156:7777;46.174.51.182:7774;89.42.88.6:7024;46.105.179.99:7777;51.89.188.188:7777;146.59.209.106:3021;146.59.209.106:7777;151.80.52.97:9999;188.165.5.201:7777;94.23.145.137:7776;176.32.37.82:7777;188.166.88.79:7777;176.32.37.25:7777;82.208.17.47:27482;164.132.200.55:2239;176.32.39.198:7777;51.91.91.105:7777;176.32.37.251:7777;176.32.39.31:7777;193.39.15.204:7778;93.185.105.240:7777;51.83.217.86:7763;185.213.174.139:7359;91.134.166.76:8085;149.202.86.51:7777;91.134.166.79:7776;80.66.71.85:7777;185.212.227.65:7777;188.165.138.225:7777;54.38.103.220:1110;217.106.106.85:7017;5.39.108.55:7777;163.172.105.21:7777;37.221.209.130:7783;91.121.87.14:3569;164.132.200.55:2463;91.134.166.77:1100;141.95.234.19:7777;81.0.217.177:27859;54.37.142.74:7777;91.134.166.76:7777;82.208.17.10:27981;51.75.232.69:3385;79.137.97.41:7777;45.125.66.167:7744;89.253.222.60:7777;51.255.61.5:7777;51.68.204.178:7777;79.137.97.49:7777;45.125.66.97:7779;136.243.89.49:7777;141.95.190.140:1741;45.125.66.97:7750;145.239.76.234:7777;45.125.66.53:8059;45.125.66.97:5566;185.169.134.163:7777;176.32.36.80:7777;185.248.140.130:2222;185.169.134.174:7777;198.251.83.10:7777;80.66.82.144:7777;176.32.39.249:7777;5.9.115.244:7777;217.182.36.95:7777;37.140.197.163:7777;153.92.1.25:7777;45.144.155.103:7778;176.32.39.13:7777;198.251.81.128:7777;62.122.215.58:7777;51.38.143.249:7777;80.66.71.64:7777;46.174.55.8:7777;45.125.66.53:8045;45.125.66.167:9911;135.181.177.214:1451;51.75.44.10:7777;135.125.188.18:7777;146.59.56.22:2023;5.252.100.31:7777;51.195.61.148:7777;141.95.190.142:7777;151.115.54.89:7777;51.178.185.228:7781;145.239.0.129:7777;51.195.61.148:3333;149.18.62.126:7777;78.31.65.74:7777;185.169.134.85:7777;82.208.16.237:7777;185.169.134.109:7777;82.208.17.10:27087;146.59.24.20:7777;94.23.168.153:6666;54.37.142.72:7777;54.36.124.11:7777;89.176.226.190:7777;91.211.244.140:7777;45.125.66.97:4433;213.227.154.203:7777;51.254.139.153:7777;37.143.10.154:5555;178.32.55.41:7791;91.121.237.128:8888;46.174.53.118:7777;146.59.53.34:7777;176.32.39.9:7777;176.32.36.131:7777;176.32.39.11:7777;46.174.51.253:7777;83.222.116.222:3333;185.169.134.67:7777;34.91.34.84:7777;80.66.82.128:7777;5.83.43.191:7777;91.215.86.33:7777;80.66.82.168:7777;185.52.0.189:7777;176.32.39.131:7777;45.144.155.91:7777;178.32.234.17:7777;46.174.48.50:7850;45.125.66.97:6000;80.96.14.135:7777;46.174.49.47:7807;81.181.129.30:7777;46.174.48.184:7777;45.125.66.167:9955;45.125.66.167:7778;46.174.50.20:7781;195.18.27.241:1611;37.230.228.149:7777;54.38.117.76:1333;193.70.40.170:7777;188.212.101.92:7777;93.114.82.90:7777;5.135.24.181:7778;37.230.137.174:7778;51.38.128.227:7777;185.169.134.91:7777;185.169.134.84:7777;176.31.198.240:7787;91.134.166.75:7778;185.169.134.5:7777;193.70.126.129:4040;65.108.215.96:7777;80.66.71.65:7777;37.221.209.216:7785;62.77.157.53:7777;62.233.46.52:7777;51.83.174.79:7777;194.67.121.221:7777;62.122.214.172:7777;195.18.27.226:9996;51.255.61.5:9999;37.230.137.174:7777;46.174.50.45:7801;46.174.48.62:7777;31.31.202.5:7777;188.165.6.206:7777;46.174.48.46:7815;195.3.145.63:7777;151.248.113.17:7777;46.174.51.182:7777;46.174.55.45:7777;46.174.54.242:7777;193.203.39.76:7777;217.106.106.85:7007;51.178.14.84:7777;78.31.65.74:7775;46.174.54.117:7777;157.254.166.0:30000;45.125.66.53:8811;193.203.39.49:7777;193.203.39.13:7777;202.61.198.12:8888;185.169.134.59:7777;51.38.103.166:7777;217.106.106.85:7003;80.66.82.132:7777;45.125.66.167:9999;31.31.77.202:8888;46.174.49.48:7789;176.32.39.36:7777;46.174.49.48:7805;46.39.225.193:9999;37.221.209.216:7778;46.183.184.33:6072;2.59.132.16:7777;91.121.87.14:3842;46.174.50.112:7777;51.178.191.128:7777;82.208.17.10:27899;194.50.24.225:7777;185.204.2.126:7777;185.189.15.89:7000;45.155.207.161:7777;62.173.154.206:6363;193.70.30.99:7777;176.32.39.33:7777;51.89.188.189:7750;194.68.59.72:7777;54.37.235.200:7777;176.32.39.28:7777;195.18.27.241:1867;176.32.39.142:7777;37.48.87.211:7777;82.202.173.62:7771;77.232.152.56:7777;37.230.137.181:7777;23.88.111.216:7777;80.66.82.200:7777;178.32.234.18:7777;176.32.39.199:7777;31.186.250.169:7777;83.222.116.222:1111;46.41.142.58:7777;195.18.27.241:1767;152.228.156.129:7777;51.83.146.96:7777;185.159.131.54:7777;80.66.82.191:7777;91.134.166.73:7777;51.83.217.86:7754;193.84.90.23:7773;62.173.154.206:1117;46.174.54.192:7777;185.189.255.97:1795;62.122.214.202:7777;46.174.50.223:7777;193.203.39.81:7777;93.115.32.3:7777;193.84.90.23:7775;91.134.166.74:7777;185.189.15.89:7778;80.66.82.190:7777;5.252.116.63:7771;62.173.154.206:5656;91.134.166.72:8888;54.37.142.73:7777;51.15.108.122:1337;80.66.71.46:5125;51.75.131.194:7777;195.62.16.163:7777;51.68.189.222:7777;5.83.43.171:7777;51.38.218.165:7716;178.63.13.186:15735;193.70.126.129:4028;80.66.82.113:7777;80.66.71.87:7777;195.18.27.241:1419;91.227.18.16:7777;93.125.121.223:7777;141.95.234.18:1249;195.18.27.241:1203;185.189.15.89:7228;164.132.200.55:2291;93.93.65.32:7777;141.94.184.107:1319;185.169.134.43:7777;82.208.16.176:7777;141.95.190.140:1181;185.169.134.172:7777;149.202.26.173:7777;51.210.111.38:7777;94.23.168.153:3333;46.174.50.86:7777;45.125.66.97:7778;185.169.134.108:7777;195.178.103.36:7777;51.91.215.125:7777;45.134.11.222:7777;37.187.196.107:7777;185.169.132.141:7777;94.23.168.153:1115;51.91.91.88:7777;185.91.116.218:7777;188.212.100.129:7777;145.239.136.61:7777;37.221.209.216:7777;45.125.66.53:9966;176.32.36.33:7777;195.18.27.241:1223;213.32.120.180:7777;51.178.18.60:7777;185.189.15.89:7084;46.39.225.193:8888;54.38.117.76:1341;46.174.48.235:7777;5.39.108.50:1195;15.235.56.74:7777;46.174.49.170:7777;79.98.109.115:7777;46.39.225.193:7777;185.189.255.97:1479;62.122.215.182:7777;51.75.44.14:1127;45.136.204.113:7777;45.125.66.107:7778;46.174.53.247:7777;178.250.189.35:7777;51.91.91.100:7777;149.56.41.53:7774;176.32.39.16:7777;54.38.156.202:7777;151.80.52.96:9700;217.106.106.85:7028;62.122.213.220:7777;37.230.162.202:7777;195.18.27.241:1451;91.215.86.34:7777;54.37.210.119:7777;185.238.73.117:27015;46.174.49.47:7794;176.32.39.165:7777;141.193.68.165:7777;45.125.66.177:8877;85.117.36.229:1125;185.183.157.239:7777;95.111.226.28:7777;45.134.11.165:7777;77.222.52.50:1111;51.83.217.86:7753;37.221.209.216:7789;62.109.1.2:7785;198.251.82.204:7777;46.174.51.182:7771;185.86.78.125:7777;164.132.200.55:1255;176.32.39.10:7777;147.135.4.40:7777;193.84.90.23:7777;45.136.205.84:7777;62.122.215.124:7777;51.83.207.243:7777;176.32.39.200:7777;51.75.44.14:1231;46.174.51.188:1129;178.63.13.150:7777;80.66.82.159:7777;217.106.106.36:7777;46.174.50.61:7777;3.87.215.153:7777;46.174.51.182:7772;217.106.106.178:7160;149.56.41.54:7790;217.106.106.86:7025;185.169.134.165:7777;176.32.36.246:7777;193.84.90.23:7772;46.174.54.184:7777;62.122.213.34:7777;195.18.27.241:1643;185.185.134.252:7777;217.106.106.85:7033;51.91.91.121:7777;62.122.214.9:7777;51.254.178.238:7777;130.162.42.73:7777;213.202.222.84:7777;45.136.204.134:7777;51.178.185.231:7777;46.174.50.20:7853;54.37.157.58:7602;91.134.166.78:1234;37.221.193.189:7777;80.66.71.47:5125;185.71.66.161:7777;46.174.51.182:7775;89.163.129.51:7777;185.31.160.187:1113;193.84.90.20:7777;5.196.112.104:7777;5.39.108.55:1189;213.32.120.114:7777;37.247.108.107:7777;151.80.243.170:7777;185.189.15.22:2369;80.66.82.188:7777;178.32.203.118:4000;45.155.207.161:7004;135.148.26.127:7777;151.80.47.38:7777;193.84.90.22:7777;193.84.90.17:7771;193.84.90.19:7777;192.99.32.172:7847;185.189.255.97:7770;193.84.90.21:7777;147.135.127.106:7777;5.196.112.104:7709;198.50.195.140:7774;135.148.70.136:7779;149.56.41.55:7774;87.98.154.133:7777;91.134.166.73:7000;194.93.2.27:7777;135.148.153.108:7777;185.71.66.173:7777;164.132.200.55:1395;135.148.89.12:7777;185.71.66.172:7777;51.83.217.86:7752;195.18.27.241:1403;80.66.82.219:7777;146.59.72.76:7777;146.59.220.31:7777;54.38.117.77:1311;149.56.237.181:7777;193.70.94.12:7777;195.18.27.241:1519;144.217.113.116:7777;54.39.150.1:7080;167.114.138.254:7777;51.83.217.86:7756;5.57.38.221:7777;51.83.217.86:7758;46.174.51.182:7773;135.148.13.61:7777;144.217.19.110:8001;81.229.194.175:7777;217.106.106.86:7002;135.148.159.205:7777;46.39.225.193:7778;195.18.27.241:1143;142.44.169.69:7777;144.217.123.12:7777;80.66.82.82:7777;137.74.6.199:4004;185.71.66.191:7777;66.70.203.212:7777;146.59.53.150:7777;51.83.174.78:7777;188.212.100.219:7777;51.222.31.3:7070;86.123.139.225:7777;144.217.10.170:7777;135.148.36.192:7777;149.56.195.234:7777;149.56.41.48:7770;80.66.71.63:7777;46.39.225.193:7779;176.32.39.138:7777;51.222.160.0:7778;46.174.50.20:7822;193.84.90.17:7772;144.217.19.111:7777;51.79.111.97:7777;193.203.39.74:7777;185.253.34.52:1137;193.203.39.80:7777;144.217.113.119:7777;45.125.66.97:7755;66.70.157.101:7777;149.56.41.50:7772;62.122.213.84:7777;149.56.46.81:7777;192.99.185.75:7777;167.114.5.193:7777;54.39.38.83:7777;144.217.123.13:7777;193.17.92.160:1134;147.135.93.209:7777;93.115.101.4:7777;144.217.10.81:7777;167.114.42.236:7777;195.18.27.241:1667;51.222.22.65:7777;46.174.48.50:7788;51.222.228.151:7777;45.136.204.32:7777;45.144.155.103:7786;54.39.150.1:8686;54.39.194.90:7777;158.69.156.104:7777;193.203.39.42:7777;185.173.179.106:7006;160.202.167.43:7777;142.44.222.93:7777;54.39.85.166:8888;135.148.153.98:7777;142.44.160.118:7777;15.235.123.105:7777;54.39.123.144:7937;149.56.80.133:7777;46.174.49.115:7777;51.222.31.3:2222;144.217.195.241:7777;149.56.41.48:7794;46.174.49.47:7783;45.125.66.167:7722;74.91.121.137:7777;45.125.66.97:1111;149.56.41.53:7786;144.217.19.105:7777;5.42.223.222:8891;51.81.116.125:7777;51.161.56.253:7777;77.244.214.151:7777;91.215.86.32:7777;20.82.141.188:7777;46.174.50.42:7787;51.161.7.174:7777;185.189.15.89:7146;185.112.101.148:7832;91.151.89.58:7777;149.56.252.173:7777;198.100.159.90:7777;46.174.50.20:7800;184.171.219.162:7777;46.174.55.113:7777;54.39.115.96:7777;149.56.181.17:7777;144.217.19.109:7777;45.136.204.69:7777;54.39.38.150:7777;213.238.177.122:7777;149.56.41.48:7782;149.56.195.176:7777;149.56.41.55:7772;194.93.2.56:7777;167.114.39.70:7777;46.174.48.94:7777;135.148.70.136:7778;93.157.172.40:27100;144.217.174.215:1328;149.56.181.16:7777;149.56.41.54:7784;144.217.48.48:7777;149.56.41.55:7782;185.112.101.148:7780;51.79.111.147:7777;144.217.178.40:7777;80.28.229.194:7777;212.109.195.181:7777;37.143.8.138:7777;51.222.31.3:8686;62.122.214.46:7777;149.56.41.48:7790;5.42.223.222:5141;194.169.160.83:7777;149.56.41.54:7770;144.217.174.215:7778;54.39.112.33:7777;54.39.130.71:7777;158.69.152.89:7777;192.99.237.10:7777;62.78.92.92:7777;185.105.237.251:7777;51.222.193.109:7777;15.204.210.239:7777;198.50.207.186:7777;149.56.41.52:7772;51.222.106.216:7777;185.71.66.86:7777;149.56.46.81:9999;149.56.41.55:7790;149.56.1.118:7771;198.50.176.8:7777;195.18.27.241:1683;149.56.242.37:7777;54.39.149.192:7720;46.174.48.194:7777;185.189.15.24:1325;149.56.41.49:7774;193.84.90.23:7111;45.134.11.34:7787;149.56.1.118:7777;144.217.174.212:7776;51.161.98.192:7777;185.71.66.174:7777;149.154.67.49:7777;45.81.18.72:7777;140.99.252.18:7780;91.215.86.10:7777;54.39.150.1:7730;135.148.89.13:7777;66.70.220.66:7777;198.50.207.187:7777;51.222.230.213:7777;149.56.41.49:7786;192.99.185.74:7777;142.44.157.152:7777;149.56.1.118:7775;175.107.244.21:7789;144.217.174.213:1234;103.77.154.108:27815;160.202.167.13:7787;54.39.150.1:9999;35.168.160.6:7777;144.217.174.212:7777;135.148.73.238:7777;192.99.177.158:7777;149.56.41.55:7776;149.56.41.50:7770;149.56.41.50:7778;149.56.41.53:7778;135.148.143.157:7777;149.56.41.49:7777;135.148.13.60:7777;84.38.182.240:7777;144.217.150.105:7777;198.27.76.170:7777;144.217.174.213:7777;149.56.252.191:7777;103.154.233.192:7777;167.114.196.182:7777;15.235.111.155:7777;149.56.93.11:7777;66.70.157.103:7777;192.95.19.227:7777;157.254.166.151:7777;64.94.101.247:6666;64.94.101.247:7777;43.228.86.96:5555;51.79.162.218:7793;103.157.97.38:7777;89.46.2.67:7007;51.79.196.201:7777;15.235.204.112:7777;139.99.65.172:7777;103.131.200.139:7777;141.11.159.52:7777;191.96.92.87:7777;51.79.181.126:7759;51.79.181.126:7777;139.99.26.47:7777;191.96.92.80:7777;103.22.181.138:7777;40.83.97.201:7779;191.252.93.243:7777;51.79.162.218:1111;51.79.162.211:7777;51.79.218.114:7778;139.99.53.53:7777;191.96.92.93:7777;191.96.92.79:7777;15.235.205.31:7777;205.178.183.229:7011;103.150.93.114:7777;51.79.197.132:8888;51.79.218.114:6666;51.79.215.88:7782;147.50.252.112:7777;147.50.240.28:7777;103.131.200.136:7777;18.229.115.127:7777;189.127.164.17:7777;148.113.8.240:7777;148.113.8.240:7799;177.54.146.232:7842;177.54.146.232:7706;177.54.146.232:7830;189.127.164.28:7777;189.1.173.236:7777;148.113.8.119:30008;177.54.146.232:7800;148.113.8.240:7801;128.199.222.6:7777;159.65.142.200:7777;40.83.121.170:7777;122.114.57.229:7777;101.100.143.138:28000;175.24.85.251:7777;58.105.108.247:7778;54.37.142.75:7777;217.10.33.74:7777;190.15.159.191:7777;54.36.82.238:6666;141.95.234.22:7777;51.75.232.64:7777;89.46.2.23:7003;176.32.36.73:7777;89.46.2.72:7777;51.83.198.113:7777;176.32.37.14:7777;144.76.57.59:35251;51.75.44.15:7777;139.162.44.92:25611;51.75.232.69:2713;89.46.2.23:7005;51.68.148.174:7777;94.23.168.153:1499;23.88.73.88:21710;91.149.165.149:7777;195.18.27.241:1523;83.4.184.29:7777;185.239.237.204:7778;164.132.200.55:1203;82.223.16.128:7777;177.91.126.194:6672;141.95.190.140:2457;89.42.88.72:1231;78.129.150.164:7777;51.75.44.8:7777;23.88.73.88:16814;37.230.162.115:7777;51.83.189.2:7777;141.95.190.139:7777;15.204.191.21:7777;164.132.200.55:2439;123.194.115.164:12345;195.18.27.226:1151;4.233.98.225:7777;51.38.207.210:1295;135.148.153.110:7777;192.99.138.63:7777;177.91.126.194:7777;209.87.250.163:7777;79.56.131.29:7777;185.189.255.97:1219;185.239.237.204:7777;131.196.199.184:7777;146.59.56.17:7777;23.88.73.88:14978;147.50.253.72:7778;195.18.27.241:1167;51.83.189.1:7777;164.132.200.55:2335;157.97.60.87:7777;46.174.53.147:7777;185.189.255.97:1865;176.32.39.15:7777;135.148.95.31:7777;145.40.73.135:7777;152.67.47.28:7777;195.18.27.241:1235;185.189.255.97:1439;143.198.199.121:3009;185.189.255.97:1443;34.93.73.65:7777;186.130.68.60:7777;109.110.50.5:7777;146.59.56.16:7777;186.130.42.189:7777;164.132.200.55:1507;149.18.62.181:7777;23.88.73.88:18059;79.27.20.8:7777;79.23.40.193:7777;66.70.175.157:7777;92.42.46.133:7777;23.88.73.88:16748;149.18.62.253:30005;135.125.128.240:7827;176.32.39.154:7777;216.238.113.189:7777;146.59.56.21:7777;198.251.81.222:7777;185.189.255.97:1111;51.83.189.6:7777;51.83.189.13:7777;103.3.62.62:7777;131.196.196.91:7777;66.70.194.206:7777;78.129.218.37:7777;45.58.127.66:7777;51.83.198.112:7777;185.189.255.97:1371;176.32.39.2:7777;185.189.255.97:1259;164.132.200.55:1423;164.132.200.55:2043;147.135.229.229:1533;45.136.205.203:7777;135.148.143.156:7777;23.88.73.88:13828;15.235.121.234:7777;23.88.73.88:22356;149.56.41.54:7798;15.204.167.100:7777;89.46.2.43:7777;89.46.2.84:7777;185.239.237.204:7779;185.255.134.155:7777;198.251.83.51:7777;23.26.135.45:7777;142.93.212.120:7777;198.251.83.222:7777;154.26.139.119:9031;149.18.62.156:7777;66.70.229.60:7777;51.75.44.14:7777;47.111.152.162:7777;175.24.85.251:8888;51.83.189.10:7777;103.166.228.31:7777;15.235.149.57:7777;158.69.122.238:7777;66.70.157.100:7777;157.254.166.205:7777;15.204.173.72:7777;144.217.62.159:7777;66.70.203.215:7777;193.84.90.23:7771;46.174.52.246:7777;80.66.82.249:7777;213.238.177.150:7777;193.84.90.23:7774;181.214.214.216:10049;51.83.189.4:7777;213.32.6.232:7777;51.91.91.99:7777;51.91.91.104:7777;51.77.70.31:7777;37.230.137.194:7777;54.38.218.179:7777;198.251.83.155:7777;149.18.62.151:7777;185.107.96.148:7777;5.45.71.161:7777;154.26.139.119:9075;141.94.20.128:7777";

        string[] ip_list = ip_text.Split(';');

        foreach(string ip in ip_list) {
            string printOut = await ServerManager.AddServer(ip);
            if (!printOut.Contains("already"))
            {
                Console.WriteLine(printOut);
            }
        }

        return true;
    }

    [HttpGet("GetGlobalMetrics")]
    public async Task<List<GlobalMetrics>> GetGlobalMetrics(int hours = 6)
    {
        DateTime RequestTime = DateTime.Now - TimeSpan.FromHours(hours);

        var conn = new MySqlConnection(MySQL.ConnectionString);
        var sql = @"SELECT players, servers, api_hits, time FROM metrics_global WHERE time > @RequestTime ORDER BY time DESC";

        return (await conn.QueryAsync<GlobalMetrics>(sql, new { RequestTime })).ToList();
    }

    [HttpGet("GetServerMetrics")]
    public async Task<dynamic> GetServerMetrics(string ip_addr = "none", int hours = 6, int include_misses = 0)
    {
        ServerManager.ApiHits++;

        DateTime RequestTime = DateTime.Now - TimeSpan.FromHours(hours);

        int Id = ServerManager.GetServerIDFromIP(ip_addr);

        var conn = new MySqlConnection(MySQL.ConnectionString);

        string sql;

        // "Misses" are times where the server was down at the time of being queried. This is recorded as having -1 players online.
        // This data might be misleading or undesired, as such, we don't include it unless explicitly requested.
        if (include_misses > 0)
        {
            sql = @"SELECT players, time FROM metrics_server WHERE time > @RequestTime AND server_id = @Id ORDER BY time DESC";
        }
        else
        {
            sql = @"SELECT players, time FROM metrics_server WHERE time > @RequestTime AND server_id = @Id AND players >= 0 ORDER BY time DESC";
        }

        return (await conn.QueryAsync<ServerMetrics>(sql, new { RequestTime, Id })).ToList();
    }
}