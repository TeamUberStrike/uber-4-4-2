using System.Collections;
using UnityEngine;

public class LotteryPopupTask
{
	private enum State
	{
		None = 0,
		Rolled = 1
	}

	private const float MinWaitingTime = 2f;

	private State _state;

	private LotteryPopupDialog _popup;

	public LotteryPopupTask(LotteryPopupDialog dialog)
	{
		_state = State.None;
		_popup = dialog;
		_popup.SetRollCallback(OnLotteryRolled);
		MonoRoutine.Start(StartTask());
	}

	private IEnumerator StartTask()
	{
		while (_state == State.None)
		{
			yield return new WaitForEndOfFrame();
		}
		if (_state == State.Rolled)
		{
			_popup.IsWaiting = true;
			_popup.IsUIDisabled = true;
			float time = 0f;
			while (_popup.ReturnedState == LotteryPopupDialog.MyState.Waiting || time < 2f)
			{
				time += Time.deltaTime;
				yield return new WaitForEndOfFrame();
			}
			_popup.IsWaiting = false;
			if (_popup.ReturnedState == LotteryPopupDialog.MyState.Success)
			{
				PrefabManager.Instance.InstantiateLotteryEffect();
				SfxManager.Play2dAudioClip(GameAudio.MysteryBoxWin);
				MonoRoutine.Start(Singleton<PlayerDataManager>.Instance.StartGetMember());
				MonoRoutine.Start(Singleton<ItemManager>.Instance.StartGetInventory(false));
				yield return new WaitForSeconds(2f);
				PopupSystem.HideMessage(_popup);
				LotteryWinningPopup winningPopup = _popup.ShowReward();
				if (winningPopup != null)
				{
					PopupSystem.Show(winningPopup);
					Animation _uiAnim = PrefabManager.Instance.GetLotteryUIAnimation();
					_uiAnim.Play("mainPopup");
					while (_uiAnim.isPlaying)
					{
						winningPopup.SetYOffset(_uiAnim.transform.position.y);
						yield return new WaitForEndOfFrame();
					}
					Object.Destroy(_uiAnim.gameObject);
				}
			}
			else
			{
				PopupSystem.HideMessage(_popup);
			}
		}
		else
		{
			PopupSystem.HideMessage(_popup);
		}
	}

	private void OnLotteryRolled()
	{
		_state = State.Rolled;
	}
}
