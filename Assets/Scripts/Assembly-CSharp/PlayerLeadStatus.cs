internal class PlayerLeadStatus : Singleton<PlayerLeadStatus>
{
	private enum LeadState
	{
		None = 0,
		Me = 1,
		Tied = 2,
		Others = 3
	}

	private LeadState _lastLead;

	public bool IsLeading
	{
		get
		{
			return _lastLead == LeadState.Me;
		}
	}

	private PlayerLeadStatus()
	{
	}

	public void ResetPlayerLead()
	{
		_lastLead = LeadState.None;
	}

	public void PlayLeadAudio(int myKills, int otherKills, bool isLeading, bool playAudio = true)
	{
		if (myKills == 0 && otherKills == 0)
		{
			_lastLead = LeadState.None;
		}
		else if (isLeading)
		{
			if (playAudio && _lastLead != LeadState.Me && myKills > 0)
			{
				SfxManager.Play2dAudioClip(GameAudio.TakenLead, 0.5f);
			}
			_lastLead = LeadState.Me;
		}
		else if (_lastLead == LeadState.Me)
		{
			_lastLead = LeadState.Tied;
			if (playAudio && myKills > 0)
			{
				SfxManager.Play2dAudioClip(GameAudio.TiedLead, 0.5f);
			}
		}
		else if (_lastLead == LeadState.Tied)
		{
			_lastLead = LeadState.Others;
			if (playAudio)
			{
				SfxManager.Play2dAudioClip(GameAudio.LostLead, 0.5f);
			}
		}
		else if (myKills == otherKills && myKills > 0)
		{
			_lastLead = LeadState.Tied;
			if (playAudio)
			{
				SfxManager.Play2dAudioClip(GameAudio.TiedLead, 0.5f);
			}
		}
		else
		{
			_lastLead = LeadState.Others;
		}
	}

	public void OnDeathMatchOver()
	{
		if (_lastLead == LeadState.Me)
		{
			SfxManager.StopAll2dAudio();
			SfxManager.Play2dAudioClip(GameAudio.YouWin, 1f);
		}
		else if (_lastLead == LeadState.Others)
		{
			SfxManager.StopAll2dAudio();
			SfxManager.Play2dAudioClip(GameAudio.GameOver, 1f);
		}
		else
		{
			SfxManager.StopAll2dAudio();
			SfxManager.Play2dAudioClip(GameAudio.Draw, 1f);
		}
	}
}
