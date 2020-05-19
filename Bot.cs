using System;
using System.Drawing;
using System.Windows.Forms;

namespace GameSeaFight
{
    class Bot
    {
        private int[,] MapMy = new int[Form1.SizeMap, Form1.SizeMap]; //массив карты бота
        private int[,] MapOpponent = new int[Form1.SizeMap, Form1.SizeMap]; //массив нашей карты (игрока)

        private Button[,] MyButtons = new Button[Form1.SizeMap, Form1.SizeMap]; //двумерный массив кнопок бота 
        private Button[,] OpponentButtons = new Button[Form1.SizeMap, Form1.SizeMap]; //двумерный массив наших кнопок (игрока)

        public Bot(int[,] mapMy, int[,] mapOpponent, Button[,] myButtons, Button[,] opponentButtons) //конструктор
        {
            this.MapMy = mapMy;
            this.MapOpponent = mapOpponent;
            this.MyButtons = myButtons;
            this.OpponentButtons = opponentButtons;
        }

        private bool IsInsideMap(int i, int j) //проверяем, находимся ли мы в рамках карты (избегаем выхода за границы массива)
        {
            if (i < 0 || j < 0 || i >= Form1.SizeMap || j >= Form1.SizeMap)
                return false;
            return true;
        }

        private bool IsEmpty(int i, int j, int length) //проверяем, не пустая ли нацеленная клетка
        {
            var isEmpty = true;
            for (var k = j; k < j + length; k++)
            {
                if (MapMy[i, k] != 0)
                {
                    isEmpty = false;
                    break;
                }
            }

            return isEmpty;
        }

        internal int[,] ConfigureShips() //функция расстановки кораблей для бота
        {
            var lengthShip = 4;
            var shipSize = 2;
            var shipsCount = 4;
            var rnd = new Random();
            while (shipsCount > 0)
            {
                for (var i = 0; i < shipSize; i++)
                {

                    var posX = rnd.Next(1, 9);
                    var posY = rnd.Next(1, 9);

                    while (!IsInsideMap(posX, posY + lengthShip - 1) || !IsEmpty(posX, posY, lengthShip)) //генерируем адекватные posX, posY
                    {
                        posX = rnd.Next(1, 9);
                        posY = rnd.Next(1, 9);
                    }

                    for (var j = 0; j < 2; j++) //создаем 2 трёх-палубных
                    {
                        if (MapMy[posX + 1, posY + 1] != 1)
                        {
                            MapMy[posX + 1, posY] = 1;
                            MapMy[posX, posY] = 1;
                            MapMy[posX - 1, posY] = 1;
                        }
                    }

                    while (!IsInsideMap(posX, posY + lengthShip - 1) || !IsEmpty(posX, posY, lengthShip)) //генерируем адекватные posX, posY
                    {
                        posX = rnd.Next(1, 9);
                        posY = rnd.Next(1, 9);
                    }
                    for (var k = 0; k < 3; k++) //создаем 3 двух-палубных
                    {
                        if (MapMy[posX + 1, posY + 1] != 1)
                        {
                            MapMy[posX, posY] = 1;
                            MapMy[posX, posY + 1] = 1;
                        }
                    }

                    while (!IsInsideMap(posX, posY + lengthShip - 1) || !IsEmpty(posX, posY, lengthShip)) //генерируем адекватные posX, posY
                    {
                        posX = rnd.Next(1, 9);
                        posY = rnd.Next(1, 9);
                    }

                    for (var j = 0; j < 4; j++) //создаем 4 одно-палубных
                    {
                        if (MapMy[posX + 1, posY + 1] != 1)
                            MapMy[posX, posY] = 1;
                    }

                    lengthShip--;
                    shipSize *= 2;
                    shipsCount--;
                    if (shipsCount <= 0)
                        break;
                }
            }
            return MapMy;
        }

        internal bool Shoot() //функция, с помощью которой осуществляется стрельба по кораблям
        {
            var hit = false;

            var rnd = new Random();

            var posX = rnd.Next(1, Form1.SizeMap);
            var posY = rnd.Next(1, Form1.SizeMap);

            while (OpponentButtons[posX, posY].Image == Image.FromFile("explosion.png") ||
                   OpponentButtons[posX, posY].Image == Image.FromFile("hole.png")) //есть ли в текущих клетках закрашенная
            {
                posX = rnd.Next(1, Form1.SizeMap);
                posY = rnd.Next(1, Form1.SizeMap);
            }
            

            if (MapOpponent[posX, posY] != 0)
            {
                hit = true;
                MapOpponent[posX, posY] = 0;
                OpponentButtons[posX, posY].Image = Image.FromFile("explosion.png");
            }
            else
            {
                hit = false;
                OpponentButtons[posX, posY].Image = Image.FromFile("hole.png");
            }

            if (hit)
                Shoot();
            return hit;
        }
    }
}
