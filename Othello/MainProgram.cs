using System;

namespace Othello
{
	internal class MainProgram
	{
		public static int turnNumber;
		public static int passCount;
		public const int GRIDSIZE = 8;
		public const int NUMBEROFSEARCHING = 1000;
		
		public static void Main(string[] args)
		{
			int[,] grids = new int[GRIDSIZE, GRIDSIZE];
			
			//初期化
			for (int i = 0; i < GRIDSIZE; i++)
			{
				for (int j = 0; j < GRIDSIZE; j++)
					grids[i, j] = 0;
			}
			//-1しているのはインデックスが0から始まるため
			grids[GRIDSIZE / 2 - 1, GRIDSIZE / 2 - 1] = grids[GRIDSIZE / 2, GRIDSIZE / 2] = 2;
			grids[GRIDSIZE / 2 - 1, GRIDSIZE / 2] = grids[GRIDSIZE / 2, GRIDSIZE / 2 - 1] = 1;
			
			//FIXME:最初の盤面描写(統合するかも)
			Process.printGrid(grids);
			Console.WriteLine();
		
			//プレイヤーの先手番・後手番の選択
			Console.WriteLine("あなたが先手番か、後手番かを選択してください");
			Console.WriteLine("先手番：0、後手番：1");
			Console.Write("手番：");
			turnNumber = int.Parse(Console.ReadLine());
			
			//プレイヤーが後手の場合、まずコンピューターに打たせる
			if (turnNumber == 1)
			{
				turnNumber = 0;
				string hand;
				hand = Process.nextHand(grids, turnNumber);
				Console.WriteLine("コンピュータが打った場所：" + hand);
				grids = Process.nextGrid(grids, hand);
				Process.printGrid(grids);
				turnNumber = 1;
			}
			
			
			//ここからゲーム開始
			bool checkmate = false;
			while (!checkmate)
			{
				Console.WriteLine("あなたの手番です");
				Console.WriteLine("アルファベット1文字+数字1文字で打つマスを指定してください");
				Console.WriteLine("あなたの色：" + (turnNumber + 1));

				string hand = "";
				passCount = 0;
				//評価値の最大が0の場合、それはどこにも打てないということなのでパス
				if (Process.searchMaxScore(grids, out int hoge, out int huga) == 0)
				{
					Console.WriteLine("打てるマスがないため、パスとなりました");
					passCount++;
				}
				else
				{
					bool canputDownPlayer = false;
					//打てないマスに打とうとした場合打ち直させる
					while (!canputDownPlayer)
					{
						Console.Write("打つ場所：");
						hand = Console.ReadLine();
						Process.shapingStr(hand, out int row, out int column);
						canputDownPlayer = Process.canPutDown(grids, row, column);
						if (canputDownPlayer) break;
						
						Console.WriteLine("そこには打てません、打ち直してください");
					}	
					grids = Process.nextGrid(grids, hand);
					Process.printGrid(grids);
				}

				Console.WriteLine();
				
				//ここからコンピュータの手番
				if (turnNumber == 0) turnNumber = 1;
				else turnNumber = 0;
				hand = Process.nextHand(grids, turnNumber);
				Console.WriteLine("コンピュータの手番です");
				//パスの場合これ以下の処理はしなくて良い
				if (Process.searchMaxScore(grids, out hoge, out huga) == 0)
				{
					Console.WriteLine("打てるマスがないため、パスとなりました");
					if (turnNumber == 0) turnNumber = 1;
					else turnNumber = 0;
					Console.WriteLine();
					passCount++;
					checkmate = Process.judgeCheckmate(grids, turnNumber);
					if (checkmate) break;
					
					continue;
				}
				Console.WriteLine("コンピュータが打った場所：" + hand);
				grids = Process.nextGrid(grids, hand);
				
				Process.printGrid(grids);
				Console.WriteLine();
				
				if (turnNumber == 0) turnNumber = 1;
				else turnNumber = 0;
			}
		}
	}
}
