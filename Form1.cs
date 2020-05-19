using System;
using System.Drawing;
using System.Windows.Forms;

namespace GameSeaFight
{
    public partial class Form1 : Form
    {
        public const int SizeMap = 10; //размер нашей карты
        private readonly int CellSize = 30; //размер ячейки
        private readonly string Alphabet = "АБВГДЕЖЗИК"; //подписываем ячейки
        private readonly int[,] MapMy = new int[SizeMap, SizeMap]; //массив нашей карты 
        private int[,] MapOpponent = new int[SizeMap, SizeMap]; //массив противника карты

        private readonly Button[,] MyButtons = new Button[SizeMap, SizeMap]; //двумерный массив наших кнопок (игрока) 
        private readonly Button[,] OpponentButtons = new Button[SizeMap, SizeMap]; //двумерный массив кнопок противника

        private bool IsPlaying = false; //нажал ли игрок на кнопку

        private Bot bot; //создаём бота

        public Form1()
        {
            
            InitializeComponent();
            this.Text = "Игра \"Морской бой\"";
            this.Font = new Font("Times New Roman", 11);

            Actions();
        }

        private void Actions() //вспомогательный метод
        {
            IsPlaying = false; 
            MakeMap();
            bot = new Bot(MapOpponent, MapMy, OpponentButtons, MyButtons);
            MapOpponent = bot.ConfigureShips();
        }

        private void MakeMap() //создаем карту
        {
            var map1 = new Label
            {
                Text = "Ваша карта",
                Font = new Font("Times New Roman", 11),
                Location = new Point(SizeMap * CellSize / 2, SizeMap * CellSize + 20)
            }; //подписываем нашу карту
            this.Controls.Add(map1);

            var map2 = new Label
            {
                Text = "Чужая карта",
                Font = new Font("Times New Roman", 11),
                Location = new Point(320 + SizeMap * CellSize / 2, SizeMap * CellSize + 20)
            }; //подписываем карту бота
            this.Controls.Add(map2);

            this.Width = SizeMap * 2 * CellSize + 50;
            this.Height = (SizeMap + 3) * CellSize + 20;
            for (var i = 0; i < SizeMap; i++)//создаем нашу карту
            {
                for (var j = 0; j < SizeMap; j++)
                {
                    MapMy[i, j] = 0;

                    var button = new Button
                    {
                        Location = new Point(j * CellSize, i * CellSize),
                        Size = new Size(CellSize, CellSize),
                        Image = Image.FromFile("paper.png")
                    };
                    if (j == 0 || i == 0) //делаем цвета и кнопки по вертикали и горизонтали
                    {
                        button.Image = Image.FromFile("blue_ground.png");
                        if (i == 0 && j > 0)
                            button.Text = Alphabet[j - 1].ToString();
                        if (j == 0 && i > 0)
                            button.Text = i.ToString();
                    }
                    else
                    {
                        button.Click += new EventHandler(ConfigureShips); //вешаем обработчик - раскрашиваем нажатую кнопку (кроме тех, где стоят буквы и цифры)
                    }

                    MyButtons[i, j] = button; //добавление кнопки в двумерный массив
                    this.Controls.Add(button);
                }
            }

            for (var i = 0; i < SizeMap; i++) //делаем карту бота
            {
                for (var j = 0; j < SizeMap; j++)
                {
                    MapMy[i, j] = 0;
                    MapOpponent[i, j] = 0;

                    var button = new Button
                    {
                        Location = new Point(330 + j * CellSize, i * CellSize),
                        Size = new Size(CellSize, CellSize),
                        Image = Image.FromFile("paper.png")
                    };
                    if (j == 0 || i == 0)
                    {
                        button.Image = Image.FromFile("pink_ground.png");
                        if (i == 0 && j > 0)
                            button.Text = Alphabet[j - 1].ToString();
                        if (j == 0 && i > 0)
                            button.Text = i.ToString();
                    }
                    else
                    {
                        button.Click += new EventHandler(PlayerShoot); //вешаем обработчик для стрельбы
                    }

                    OpponentButtons[i, j] = button;
                    this.Controls.Add(button);
                }
            }

            var startButton = new Button {Text = "Начать", Font = new Font("Times New Roman", 11), BackColor = Color.Lavender}; //создаем кнопку начать
            startButton.Click += new EventHandler(Start);
            startButton.Location = new Point(0, SizeMap * CellSize + 10);
            this.Controls.Add(startButton);
        }

        private void Start(object sender, EventArgs e) //обработчик нажатия на кнопку
        {
            IsPlaying = true;
        }

        private bool CheckIfMapIsNotEmpty() //проверяем, есть ли на карте живые корабли
        {
            var isEmpty1 = true; //для карты бота
            var isEmpty2 = true; //для карты игрока
            for (var i = 1; i < SizeMap; i++)
            {
                for (var j = 1; j < SizeMap; j++)
                {
                    if (MapMy[i, j] != 0)
                        isEmpty1 = false;
                    if (MapOpponent[i, j] != 0)
                        isEmpty2 = false;
                }
            }
            return !isEmpty1 && !isEmpty2;
        }

        private void ConfigureShips(object sender, EventArgs e) //обработчик для всех кнопок нашей карты (функция расстановки кораблей)
        {
            var pressedButton = sender as Button;
            if (!IsPlaying)
            {
                if (MapMy[pressedButton.Location.Y / CellSize, pressedButton.Location.X / CellSize] == 0)
                {
                    pressedButton.Image = Image.FromFile("ship.png");
                    MapMy[pressedButton.Location.Y / CellSize, pressedButton.Location.X / CellSize] = 1; //нажатой кнопке присваиваем единицу
                }
                else
                {
                    pressedButton.Image = Image.FromFile("paper.png");
                    MapMy[pressedButton.Location.Y / CellSize, pressedButton.Location.X / CellSize] = 0;
                }
            }
        }

        private void PlayerShoot(object sender, EventArgs e) //обработчик для стрельбы
        {
            var pressedButton = sender as Button;
            var playerTurn = Shoot(MapOpponent, pressedButton);
            if (!playerTurn)
                bot.Shoot();
            if (!CheckIfMapIsNotEmpty())
            {
                this.Controls.Clear();
                Actions();
            }
        }

        private bool Shoot(int[,] map, Button pressedButton) //функция, с помощью которой осуществляется стрельба по кораблям
        {
            var hit = false;
            if (IsPlaying)
            {
                var delta = 0;
                if (pressedButton.Location.X > 320) delta = 320;
                if (map[pressedButton.Location.Y / CellSize, (pressedButton.Location.X - 320) / CellSize] != 0) //есть ли у нас что-то по координате нажато кнопки
                {
                    hit = true;
                    map[pressedButton.Location.Y / CellSize, (pressedButton.Location.X - delta) / CellSize] = 0;
                    pressedButton.Image = Image.FromFile("explosion.png");
                }
                else
                {
                    hit = false;
                    pressedButton.Image = Image.FromFile("hole.png");
                }
            }
            return hit;
        }
    }
}
