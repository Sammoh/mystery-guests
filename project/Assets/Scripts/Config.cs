using UnityEngine;

namespace Trigger
{
	internal static partial class Config
	{
		public const int ReferenceHeight = 1920;
		public const int ReferenceWidth = 1080;

		public static float ScreenRatioHbyW { get { return (float) Screen.height / Screen.width; } }
		public static float ScreenRatioWbyH { get { return (float) Screen.width / Screen.height; } }

#if UNITY_ANDROID
		public const string Platform = "android";
#elif UNITY_IOS
		public const string Platform = "ios";
#else
		public const string Platform = "default";
#endif
		
		public const float BannerUptime = 2.0f; 

		public const int RequiredSpaceToDownloadDataFiles = 1024 * 1024; 
		public const string DataVersion = "0.1";
		
		// Legacy: Uses a public url for download (do not use for sensitive data) 
		public static readonly string ServerPath = "https://trigger-dev-public.s3-us-west-1.amazonaws.com/Boilerplate/" + DataVersion + "/";
		public static readonly string BundlePath = ServerPath + Platform + "/";
		
		// Uses s3 services to make a signed request.
		public const string FolderName = "Boilerplate";
		public static readonly string S3Path = FolderName + "/" + DataVersion + "/";
		
		public static readonly string[] DataFiles = { "data/config.json"};
	}
}