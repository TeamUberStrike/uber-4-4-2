public interface ITutorialStateTransitionObserver
{
	void TransitFromInitToAirlockCameraFollowOnWaitForSeconds();

	void TransitFromAirlockCameraFollowToAirlockWelcomeOnWaitForSeconds();

	void TransitFromAirlockWelcomeToAirlockMouseLookOnWaitForSeconds();

	void TransitFromAirlockMouseLookToAirlockWASDWalkOnMouseLookAirlock();

	void TransitFromAirlockWASDWalkToAirlockDoorOpenOnWASDWalkAirlock();

	void TransitFromAirlockDoorOpenToTunnelOnEnterArmory();

	void TransitFromTunnelToArmoryOnEnterArmory();

	void TransitFromArmoryToHeadingToFightOnLeaveArmory();

	void TransitFromHeadingToFightToArenaFieldOnEnterPlayground();

	void TransitFromArenaFieldToFiniOnAllBotsKilled();
}
