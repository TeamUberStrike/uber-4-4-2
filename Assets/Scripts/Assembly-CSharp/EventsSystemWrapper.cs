using System;
using System.Collections;
using System.Text;
using MiniJSON;
using UnityEngine;

public class EventsSystemWrapper
{
	public const string eventsUrl = "http://events.cmune.com/new";

	public const float session_idle_time = 3600f;

	public bool IsDevelopment { get; set; }

	public string SessionId { get; private set; }

	public EventsSystemWrapper()
	{
		IsDevelopment = false;
		SessionId = string.Empty;
		AutoMonoBehaviour<EventTracker>.Instance.Reset();
	}

	public void SendStartClient()
	{
		SessionId = Guid.NewGuid().ToString();
		Hashtable values = new Hashtable();
		Send("start_client", values);
	}

	public void SendConfigLoaded(int platform, string webservicesUrl)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["platform"] = platform;
		hashtable["webservices_url"] = webservicesUrl;
		Send("config_loaded", hashtable);
	}

	public void SendLoadingFinished()
	{
		Hashtable values = new Hashtable();
		Send("finish_loading", values);
	}

	public void SendLoadingError()
	{
		Hashtable values = new Hashtable();
		Send("finish_loading_error", values);
	}

	public void SendInClientRegistration()
	{
		Hashtable values = new Hashtable();
		Send("in_client_registration", values);
	}

	public void SendInClientRegistrationInvalid(string error)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["error"] = error;
		Send("in_client_registration_invalid", hashtable);
	}

	public void SendInClientRegistrationError()
	{
		Hashtable values = new Hashtable();
		Send("in_client_registration_error", values);
	}

	public void SendLogin(int cmid)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["cmid"] = cmid;
		Send("login", hashtable);
	}

	public void SendLoginError()
	{
		Hashtable values = new Hashtable();
		Send("login_error", values);
	}

	public void SendLoginInvalid(string error)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["error"] = error;
		Send("login_invalid", hashtable);
	}

	public void SendLogout(int cmid)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["cmid"] = cmid;
		Send("logout", hashtable);
	}

	public void SendFinishLoadingApplicationData(int cmid)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["cmid"] = cmid;
		Send("finish_loading_application_data", hashtable);
	}

	public void SendFinishLoadingApplicationDataError(int cmid)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["cmid"] = cmid;
		Send("finish_loading_application_data_error", hashtable);
	}

	public void SendStartTutorial(int cmid)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["cmid"] = cmid;
		Send("start_tutorial", hashtable);
	}

	public void SendSkipTutorial(int cmid)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["cmid"] = cmid;
		Send("skip_tutorial", hashtable);
	}

	public void SendFinishTutorial(int cmid)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["cmid"] = cmid;
		Send("finish_tutorial", hashtable);
	}

	public void SendChoosePlayerName(int cmid)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["cmid"] = cmid;
		Send("choose_player_name", hashtable);
	}

	public void SendChoosePlayerNameInvalid(int cmid, string error)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["cmid"] = cmid;
		hashtable["error"] = error;
		Send("choose_player_name_invalid", hashtable);
	}

	public void SendChoosePlayerNameError(int cmid)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["cmid"] = cmid;
		Send("choose_player_name_error", hashtable);
	}

	public void SendFacebookAppRequest(string type, int cmid, string targetFacebookId)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["source_cmid"] = cmid;
		hashtable["target_facebook_id"] = targetFacebookId;
		Send(type, hashtable);
	}

	public void SendGiftFacebookReceived(int cmid, string senderFacebookId, string itemId, string status)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["source_cmid"] = cmid;
		hashtable["sender_facebook_id"] = senderFacebookId;
		hashtable["item_id"] = senderFacebookId;
		hashtable["status"] = senderFacebookId;
		Send("gift_facebook_received", hashtable);
	}

	public void SendCreateScreenshot(int cmid, string facebookId)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["source_cmid"] = cmid;
		hashtable["facebook_id"] = facebookId;
		Send("create_screenshot", hashtable);
	}

	public void SendJoinGame(int cmid, string gameId, int mapId, int duration, short mode, int limit)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["cmid"] = cmid;
		hashtable["gid"] = gameId;
		hashtable["game_map_id"] = mapId;
		hashtable["game_duration"] = duration;
		hashtable["game_mode"] = mode;
		hashtable["game_time_limit"] = limit;
		Send("join_game", hashtable);
	}

	public void SendJoinFailed(int cmid, string gameId, string error)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["cmid"] = cmid;
		hashtable["gid"] = gameId;
		hashtable["error"] = error;
		Send("join_game_failed", hashtable);
	}

	public void SendCreateGame(int cmid, int mapId, int duration, short mode, int limit)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["cmid"] = cmid;
		hashtable["game_map_id"] = mapId;
		hashtable["game_duration"] = duration;
		hashtable["game_mode"] = mode;
		hashtable["game_time_limit"] = limit;
		Send("create_game", hashtable);
	}

	public void SendPlayerDied(int cmid, string gameId, string matchId, int killerCmid, int weaponId)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["cmid"] = cmid;
		hashtable["gid"] = gameId;
		hashtable["mid"] = matchId;
		hashtable["killer_cmid"] = killerCmid;
		hashtable["weapon"] = weaponId;
		Send("player_died", hashtable);
	}

	public void SendPlayerKill(int cmid, string gameId, string matchId, int victimCmid, int weaponId)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["cmid"] = cmid;
		hashtable["gid"] = gameId;
		hashtable["mid"] = matchId;
		hashtable["victim_cmid"] = victimCmid;
		hashtable["weapon"] = weaponId;
		Send("player_kill", hashtable);
	}

	public void SendPlayerSuicide(int cmid, string gameId, string matchId)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["cmid"] = cmid;
		hashtable["gid"] = gameId;
		hashtable["mid"] = matchId;
		Send("player_suicide", hashtable);
	}

	public void SendFinishMatch(int cmid, string gameId, string matchId)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["cmid"] = cmid;
		hashtable["gid"] = gameId;
		hashtable["mid"] = matchId;
		Send("finish_match", hashtable);
	}

	public void SendLeaveGame(int cmid, string gameId, int matchesPlayed, bool isEndOfRound)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["cmid"] = cmid;
		hashtable["gid"] = gameId;
		hashtable["completed_match"] = isEndOfRound;
		hashtable["matches_played"] = matchesPlayed;
		Send("leave_game", hashtable);
	}

	public void SendGameDisconnected(int cmid, string gameId, string error)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["cmid"] = cmid;
		hashtable["gid"] = gameId;
		hashtable["error"] = error;
		Send("game_disconnected", hashtable);
	}

	public void SendSessionInMenu(int cmid, string page, int clicks, float time)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["cmid"] = cmid;
		hashtable["page"] = page;
		hashtable["clicks"] = clicks;
		hashtable["time_in_page"] = time;
		Send("menu_session", hashtable);
	}

	public void SendAppBackgrounded()
	{
		Hashtable values = new Hashtable();
		Send("background_app", values);
	}

	public void SendAppReturnFromBackground(float timeAway, bool isInGame)
	{
		if (timeAway > 3600f && !isInGame)
		{
			SessionId = Guid.NewGuid().ToString();
		}
		Hashtable values = new Hashtable();
		Send("unbackground_app", values);
	}

	public void SendAppQuit()
	{
		Hashtable values = new Hashtable();
		Send("app_quit", values);
	}

	private void Send(string type, Hashtable values)
	{
		if (string.IsNullOrEmpty(SessionId))
		{
			Debug.LogError("Trying to record events but no session has been created.");
		}
		string url = "http://events.cmune.com/new";
		values["type"] = "client:" + type;
		values["sid"] = SessionId;
		values["time"] = Time.realtimeSinceStartup;
		string json = Json.Serialize(values);
		AutoMonoBehaviour<MonoRoutine>.Instance.StartCoroutine(SendJsonToUrl(url, json));
	}

	private IEnumerator SendJsonToUrl(string url, string json)
	{
		byte[] postData = Encoding.ASCII.GetBytes(json);
		Hashtable headers = new Hashtable(2);
		headers["Content-Type"] = "application/json";
		headers["Content-Length"] = postData.Length;
		yield return new WWW(url, postData, headers);
		if (IsDevelopment)
		{
			Debug.Log("Sending : " + url + " with json: " + json);
			string devUrl = url + "?type=notification&message=" + WWW.EscapeURL(json);
			yield return new WWW(devUrl);
		}
	}
}
