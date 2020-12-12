
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace voiceTracer.Hubs
{
    public class ChatHub : Hub
    {
        #region snippet_SendMessage
        //public async Task SendMessage(string user, string user_hash, string message, string hash, string datetime)
        //{
        //    await Clients.All.SendAsync("ReceiveMessage", user, user_hash, message, hash, datetime);
        //}

        Hashtable UserList = new Hashtable();

        /// <summary>
        /// ユーザ参加
        /// </summary>
        /// <param name="user"></param>
        /// <param name="group_name"></param>
        /// <returns></returns>
        public async Task Join(string user, string group_name)
        {
            string groupname = System.Web.HttpUtility.UrlDecode(group_name);            
            await Groups.AddToGroupAsync(Context.ConnectionId, createHash(groupname));            
            await Clients.Client(Context.ConnectionId).SendAsync("SystemMessage", "<b>このソフトウェアは<a href=\"https://www.google.com/intl/ja_jp/chrome/\" target=\"_blank\">GoogleChrome PC版</a>以外では動作しませんのでご注意ください。</b>また、この会議内容は一切保存されません。ページ遷移で削除されますのでお気を付けください。", "下記のメニューよりCSVでの出力は可能です。");
            await Clients.Group(createHash(groupname)).SendAsync("SystemMessage", user + "が 会議室「" + groupname + "」に 参加しました。", getTime());


            //await Clients.Client(Context.ConnectionId).SendAsync("SystemMessage","参加者：", getTime());

            UserList[Context.ConnectionId] = user;
        }
        /// <summary>
        /// ユーザ退室
        /// </summary>
        /// <param name="user"></param>
        /// <param name="group_name"></param>
        /// <returns></returns>
        public async Task Leave(string user, string group_name)
        {
            string groupname = System.Web.HttpUtility.UrlDecode(group_name);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, createHash(groupname));
            await Clients.Group(createHash(groupname)).SendAsync("SystemMessage", user + "が退出しました。", getTime());
        }

        public async Task ChangeName(string old_user, string new_user, string group_name)
        {
            string groupname = System.Web.HttpUtility.UrlDecode(group_name);
            await Clients.Group(createHash(groupname)).SendAsync("SystemMessage", old_user + "が 名前を「" + new_user + "」に 変更しました。", getTime());
        }

        /// <summary>
        /// グループにメッセージを送信
        /// </summary>
        /// <param name="group_name"></param>
        /// <param name="user"></param>
        /// <param name="user_hash"></param>
        /// <param name="message"></param>
        /// <param name="hash"></param>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public async Task SendGroupMessage(string group_name, string user, string user_hash, string message, string hash, string datetime, string type)
        {
            string groupname = System.Web.HttpUtility.UrlDecode(group_name);
            await Clients.Group(createHash(groupname)).SendAsync("ReceiveMessage", user, user_hash, message, hash, datetime, type);
        }

        public async Task FixGroupMessage(string group_name, string user, string user_hash, string message, string hash, string datetime, string type)
        {
            string groupname = System.Web.HttpUtility.UrlDecode(group_name);
            await Clients.Group(createHash(groupname)).SendAsync("FixMessage", user, user_hash, message, hash, datetime, type);
        }

        #endregion

        private string createRandHash()
        {
            var bytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
        private string getTime()
        {
            return DateTime.Now.ToString();
        }

        private string createHash(string str)
        {
            using SHA256 sha256 = SHA256.Create();

            // 文字列をバイト配列にエンコードします。
            byte[] encoded = System.Text.Encoding.UTF8.GetBytes(str);

            // ハッシュ値を計算します。
            byte[] hash = sha256.ComputeHash(encoded);

            return string.Concat(hash.Select(b => $"{b:x2}"));
        }
    }
}
