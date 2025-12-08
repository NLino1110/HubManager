namespace DMOrders.Pages.Base.Sion;

public partial class MiniMemoryGame : ContentView
{
    private const int Rows = 4;
    private const int Cols = 4;

    private readonly List<Card> Cards = new();
    private readonly Random Rand = new();

    private Card? firstCard;
    private Card? secondCard;
    private bool isBusy = false;

    public GameDrawable Drawable { get; private set; }
    public MiniMemoryGame()
	{
		InitializeComponent();
        Drawable = new GameDrawable(Cards);

        InitGame();
        GameView.Drawable = Drawable;
    }

    private void InitGame()
    {
        Cards.Clear();

        // 8 formas (2 de cada una)
        var shapes = new[]
        {
            ShapeType.Circle,
            ShapeType.Square,
            ShapeType.Triangle,
            ShapeType.Star,
            ShapeType.Cross,
            ShapeType.Xmark,
            ShapeType.Diamond,
            ShapeType.Ring
        };

        var all = shapes.Concat(shapes).ToList();
        all = all.OrderBy(x => Rand.Next()).ToList();

        for (int i = 0; i < Rows * Cols; i++)
        {
            Cards.Add(new Card
            {
                Shape = all[i],
                IsRevealed = false,
                IsMatched = false
            });
        }
    }

    private async void OnTapped(object sender, TappedEventArgs e)
    {
        if (isBusy) return;

        var point = e.GetPosition((View)sender);
        if (point == null) return;

        var card = Drawable.GetCardFromPoint(point.Value, GameView.Width, GameView.Height, Rows, Cols);
        if (card == null || card.IsRevealed) return;

        card.IsRevealed = true;
        GameView.Invalidate();

        if (firstCard == null)
        {
            firstCard = card;
            return;
        }

        if (secondCard == null)
        {
            secondCard = card;
            isBusy = true;

            // Comparar
            if (firstCard.Shape == secondCard.Shape)
            {
                firstCard.IsMatched = true;
                secondCard.IsMatched = true;
                ResetTurn();
            }
            else
            {
                // Esperar antes de esconderlas
                await Task.Delay(700);
                firstCard.IsRevealed = false;
                secondCard.IsRevealed = false;
                ResetTurn();
            }

            GameView.Invalidate();
            isBusy = false;
        }
    }

    private void ResetTurn()
    {
        firstCard = null;
        secondCard = null;
    }
}