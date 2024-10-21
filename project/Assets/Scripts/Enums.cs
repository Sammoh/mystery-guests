namespace Trigger
{
	/// <summary>
	/// WARNING : Name your state animations the same as these or MobileAppStateBehaviour
	/// won't be able to find the correct state to go to
	/// </summary>
	public enum MobileAppState
	{
		None = 0,
		Transition = 1,
		Load = 2,
		Failure = 3,
		Home = 4,
		AR = 5
	}

	public enum TransitionStyle
	{
		None = 0,
		Hidden = 1,
		FadeLeft = 2,
		FadeRight = 3,
		FadeOver = 4,
		FadeUnder = 5
	}

	public enum Background
	{
		None = 0,
		Splash = 1,
		Black = 2,
		CameraFeedAr = 3 
	}

	public enum Foreground
	{
		None = 0,
		Splash = 1
	}
	
	public enum Popup
	{
		None = 0,
		Permission_CameraMic_Request = 1,
		Permission_CameraMic_Denied = 2,
		Help = 3,
		Placement_Instructions = 4,
		enableAR = 5,
		HoleFound = 6,
		BoneFound = 7,
		BoneNotFound = 8,
		FetchPassed = 9,
		FetchFailed = 10,
		WalkFinish = 11,
		WalkContinue = 12
	}
	
	public enum Header
	{
		None = 0,
		Exit = 1,
		Close = 2,
		Exit_Help = 3
	}

	public enum Banner
	{
		None = 0,
		A = 1,
		B = 2,
		C = 3
	}

	public enum InteractableObjectState
	{
		None,
		Interacted,
		BoneFound,
		BoneNotFound,
		FrisbeeFetched,
		FrisbeeNotFetched,
		// switch to these states
		// replace all instances and just check for MinigameManager.CurrentMinigame.GameType
		Passed,
		Failed
	}

	public enum PlayerState
	{
		PlacingStage,
		CountDown,
		PreShot,
		AimingShot,
		BallInMotion,
		BallStopped,
		Scored,
		FinishedLevel,
		LoadingScreen,
		WaitingForStagePlacement,
		OnboardingWelcome,
		OnboardingTutorial,
		OnboardingStandby,
		QRScan,
		FindingPlane
	}
}