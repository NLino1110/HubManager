using System.Windows.Input;

namespace DMOrders.Controls.Behaviors
{
    public class SafeTapBehavior : Behavior<View>
    {
        private bool _isProcessing;

        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(SafeTapBehavior));

        public static readonly BindableProperty CommandParameterProperty =
            BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(SafeTapBehavior));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        protected override void OnAttachedTo(View bindable)
        {
            base.OnAttachedTo(bindable);

            bindable.BindingContextChanged += OnBindingContextChanged;
            BindingContext = bindable.BindingContext;

            var tap = new TapGestureRecognizer();
            tap.Tapped += OnTapped;

            bindable.GestureRecognizers.Add(tap);
        }

        private void OnBindingContextChanged(object sender, EventArgs e)
        {
            BindingContext = ((View)sender).BindingContext;
        }

        private async void OnTapped(object sender, EventArgs e)
        {
            if (_isProcessing) return;

            _isProcessing = true;

            try
            {
                if (Command?.CanExecute(CommandParameter) == true)
                    Command.Execute(CommandParameter);
            }
            finally
            {
                await Task.Delay(450);
                _isProcessing = false;
            }
        }
    }
}