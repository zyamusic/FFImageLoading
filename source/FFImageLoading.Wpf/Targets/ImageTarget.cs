using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using FFImageLoading.Work;

namespace FFImageLoading.Targets
{
    public class ImageTarget : Target<BitmapSource, Image>
    {
        private readonly WeakReference<Image> _controlWeakReference;

        public ImageTarget(Image control)
        {
            _controlWeakReference = new WeakReference<Image>(control);
        }

        public override bool IsValid
        {
            get
            {
                return Control != null;
            }
        }

        public override void SetAsEmpty(IImageLoaderTask task)
        {
            var control = Control;
            if (control == null)
                return;

            control.Source = null;
        }

        public override void Set(IImageLoaderTask task, BitmapSource image, bool animated)
        {
            if (task.IsCancelled)
                return;

            var control = Control;
            if (control == null || control.Source == image)
                return;

            var parameters = task.Parameters;

            if (animated)
            {
                // fade animation
                int fadeDuration = parameters.FadeAnimationDuration.HasValue ?
                    parameters.FadeAnimationDuration.Value : ImageService.Instance.Config.FadeAnimationDuration;
				DoubleAnimation fade = new DoubleAnimation
				{
					Duration = TimeSpan.FromMilliseconds(fadeDuration),
					From = 0f,
					To = 1f,
					EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut }
				};

				var fadeInStoryboard = new Storyboard();
                Storyboard.SetTargetProperty(fade, new PropertyPath("Opacity"));
                Storyboard.SetTarget(fade, control);
                fadeInStoryboard.Children.Add(fade);
                fadeInStoryboard.Begin();
                control.Source = image;
                if (IsLayoutNeeded(task))
                    control.UpdateLayout();
            }
            else
            {
                control.Source = image;
                if (IsLayoutNeeded(task))
                    control.UpdateLayout();
            }
        }

		private bool IsLayoutNeeded(IImageLoaderTask task)
        {
            if (task.Parameters.InvalidateLayoutEnabled.HasValue)
            {
                if (!task.Parameters.InvalidateLayoutEnabled.Value)
                    return false;
            }
            else if (!task.Configuration.InvalidateLayout)
            {
                return false;
            }

            return true;
        }

        public override Image Control
        {
            get
            {
				if (!_controlWeakReference.TryGetTarget(out var control))
					return null;

				if (control == null)
                    return null;

                return control;
            }
        }
    }
}
