using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace XGRAB.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				resourceMan = new ResourceManager("XGRAB.Properties.Resources", typeof(Resources).Assembly);
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static Bitmap arrow_67_512 => (Bitmap)ResourceManager.GetObject("arrow_67_512", resourceCulture);

	internal static Bitmap Bethesda_Generic_Background => (Bitmap)ResourceManager.GetObject("Bethesda_Generic_Background", resourceCulture);

	internal static Bitmap BuddyPassModal_Background => (Bitmap)ResourceManager.GetObject("BuddyPassModal_Background", resourceCulture);

	internal static Bitmap BuddyPassModal_BackgroundNoRipples => (Bitmap)ResourceManager.GetObject("BuddyPassModal_BackgroundNoRipples", resourceCulture);

	internal static Bitmap FirstRunModal_Background => (Bitmap)ResourceManager.GetObject("FirstRunModal_Background", resourceCulture);

	internal static Bitmap GradientBackground_CloudRGG => (Bitmap)ResourceManager.GetObject("GradientBackground_CloudRGG", resourceCulture);

	internal static Bitmap headset_5121 => (Bitmap)ResourceManager.GetObject("headset-5121", resourceCulture);

	internal static Bitmap RewardsSignUpBanner => (Bitmap)ResourceManager.GetObject("RewardsSignUpBanner", resourceCulture);

	internal static Bitmap SubscriptionCardBackground => (Bitmap)ResourceManager.GetObject("SubscriptionCardBackground", resourceCulture);

	internal Resources()
	{
	}
}
