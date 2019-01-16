using System;

namespace Othello
{	
	public class Process
	{	
		//次の手を考える関数
		public static string nextHand(int[,] grids, int turnNumber)
		{
			//FIXME:マスごとの評価値(もっといい言い方があるかも？)
			int[,] scoreOfGrid = new int[MainProgram.GRIDSIZE, MainProgram.GRIDSIZE];
			string hand;
			int maxScore;
			int rowOfMaxScore;
			int columnOfMaxScore;
			//それぞれのマスの評価値を計算する処理
			for (int row = 0; row < MainProgram.GRIDSIZE; row++)
			{
				for (int column = 0; column < MainProgram.GRIDSIZE; column++)
				{
					//そもそも置けない場合、それ以上やっても意味はないので次へ
					if (canPutDown(grids, row, column) == false)
					{
						//どんな評価値が出てもこれを下回らないようにする(置けないマスが選ばれてはだめなので)
						scoreOfGrid[row, column] = -1024;
						continue;
					}	
					//一旦そのマスの評価値を計算
					scoreOfGrid[row, column] = calculateScoreOfGrid(grids, row, column);
					
					//端だった場合、評価値を加算
					scoreOfGrid[row, column] += plusOrMinusScore(row, column);
					//打たれたと仮定し、盤面(のコピー)を更新
					hand = shapingNumber(row, column);
					int[,] copyGrid = copyArray(grids);
					copyGrid = nextGrid(copyGrid, hand);
					//ここから数手先までを予測、評価値を足すor引く
					int pass;
					for (int i = 0; i < MainProgram.NUMBEROFSEARCHING; i++)
					{
						pass = 0;
						if (MainProgram.turnNumber == 0) MainProgram.turnNumber = 1;
						else MainProgram.turnNumber = 0;
						
						//次は敵の手番のため、引く
						maxScore = searchMaxScore(copyGrid, out rowOfMaxScore, out columnOfMaxScore);
						if (maxScore != 0)
						{
							scoreOfGrid[row, column] -= maxScore;
							hand = shapingNumber(rowOfMaxScore, columnOfMaxScore);
							copyGrid = nextGrid(copyGrid, hand);
							
							//端の場合引く
							scoreOfGrid[row, column] -= plusOrMinusScore(rowOfMaxScore, columnOfMaxScore);
						}
						else
							pass++;
						
						if (MainProgram.turnNumber == 0) MainProgram.turnNumber = 1;
						else MainProgram.turnNumber = 0;

						maxScore = searchMaxScore(copyGrid, out rowOfMaxScore, out columnOfMaxScore);
						//その次の自分の手番のため、足す
						if (maxScore != 0)
						{
							scoreOfGrid[row, column] += maxScore;
							hand = shapingNumber(rowOfMaxScore, columnOfMaxScore);
							copyGrid = nextGrid(copyGrid, hand);

							scoreOfGrid[row, column] += plusOrMinusScore(rowOfMaxScore, columnOfMaxScore);
						}
						else
							pass++;
						
						//2回打てなかった場合、もう探索は意味なし
						if(pass == 2) break;
					}
					
				}
			}
			
			//最高の評価値のマスの行、列を探索する処理
			//FIXME:これをsearchMaxScoreに統合しようとしたところ、打てないところに打つバグが出たため冗長だが暫定的にここに置いている
			maxScore = scoreOfGrid[0, 0];
			rowOfMaxScore = 0;
			columnOfMaxScore = 0;
			for (int row = 0; row < MainProgram.GRIDSIZE; row++)
			{
				for (int column = 0; column < MainProgram.GRIDSIZE; column++)
				{
					if (maxScore < scoreOfGrid[row, column])
					{
						maxScore = scoreOfGrid[row, column];
						rowOfMaxScore = row;
						columnOfMaxScore = column;
					}
				}
			}
			hand = shapingNumber(rowOfMaxScore, columnOfMaxScore);
			return hand;
		}
		
		//次の手を打ったあとの盤面状態を返却する関数
		public static int[,] nextGrid(int[,] grids, string nextHand)
		{
			shapingStr(nextHand, out int row, out int column);

			int stone = MainProgram.turnNumber + 1;
			grids[row, column] = stone;
			
			//ここからひっくり返す処理
			if (canPutDown(grids, row, column, 1, 0))
				grids = reverse(grids,row, column, 1, 0);// 右
			if (canPutDown(grids, row, column, 0, 1))
				grids = reverse(grids, row, column, 0, 1); // 下
			if (canPutDown(grids, row, column, -1, 0))
				grids = reverse(grids, row, column, -1, 0); // 左
			if (canPutDown(grids, row, column, 0, -1))
				grids = reverse(grids, row, column, 0, -1); // 上
			if (canPutDown(grids, row, column, 1, 1))
				grids = reverse(grids, row, column, 1, 1); // 右下
			if (canPutDown(grids, row, column, -1, -1))
				grids = reverse(grids, row, column, -1, -1); // 左上
			if (canPutDown(grids, row, column, 1, -1))
				grids = reverse(grids, row, column, 1, -1); // 右上
			if (canPutDown(grids, row, column, -1, 1))
				grids = reverse(grids, row, column, -1, 1); // 左下
						
			return grids;
		}
		
		//勝敗を判断する関数
		public static bool judgeCheckmate(int[,] grids, int turnNumber)
		{
			//2回パスされていない場合、まだ打てるマスがあるということ
			if (MainProgram.passCount != 2)
				return false;
			
			int blackCount = 0;
			int WhiteCount = 0;
			for (int row = 0; row < MainProgram.GRIDSIZE; row++) {
				for (int column = 0; column < MainProgram.GRIDSIZE; column++)
				{
					//点数計算用に石の数をカウントしておく
					if (grids[row, column] == 1) blackCount++;
					if (grids[row, column] == 2) WhiteCount++;
				}
			}
			
			Console.WriteLine("チェックメイト");
			if (blackCount > WhiteCount)
			{
				Console.WriteLine("勝ち：黒");
				Console.WriteLine("ポイント：" + (blackCount - WhiteCount));
			}
			else if(blackCount < WhiteCount)
			{
				Console.WriteLine("勝ち：白");
				Console.WriteLine("ポイント：" + (WhiteCount - blackCount));
			}
			else
				Console.WriteLine("引き分け");

			return true;
		}
		
		//四隅の場合、評価値を足す(あるいは引く)関数
		public static int plusOrMinusScore(int row, int column)
		{
			int score = 0;
			if (row == 0 || row == 7) score += 5;
			if (column == 0 || column == 7) score += 5;
			return score;
		}
		
		//指定されたマスの評価値を計算する関数(その盤面のみ対象)
		public static int calculateScoreOfGrid(int[,] grids, int row, int column)
		{
			int score = 0;
			if (!canPutDown(grids, row, column))
				return -1024;

			if (canPutDown(grids, row, column, 1, 0))
				score += checkScoreInDirection(grids, row, column, 1, 0); // 右
			if (canPutDown(grids, row, column, 0, 1))
				score += checkScoreInDirection(grids, row, column, 0, 1); // 下
			if (canPutDown(grids, row, column, -1, 0))
				score += checkScoreInDirection(grids, row, column, -1, 0); // 左
			if (canPutDown(grids, row, column, 0, -1))
				score += checkScoreInDirection(grids, row, column, 0, -1); // 上
			if (canPutDown(grids, row, column, 1, 1))
				score += checkScoreInDirection(grids, row, column, 1, 1); // 右下
			if (canPutDown(grids, row, column, -1, -1))
				score += checkScoreInDirection(grids, row, column, -1, -1); // 左上
			if (canPutDown(grids, row, column, 1, -1))
				score += checkScoreInDirection(grids, row, column, 1, -1); // 右上
			if (canPutDown(grids, row, column, -1, 1))
				score += checkScoreInDirection(grids, row, column, -1, 1); // 左下
				
			return score;
		}
		
		//渡された盤面の最大の評価値、その行番号、列番号を検索する関数(その盤面のみ対象)
		public static int searchMaxScore(int[,] grids, out int rowOfMaxScore, out int columnOfMaxScore)
		{
			int[,] scoreOfGrids = grids;
			scoreOfGrids = new int[MainProgram.GRIDSIZE, MainProgram.GRIDSIZE];
			for (int row = 0; row < MainProgram.GRIDSIZE; row++)
			{
				for (int column = 0; column < MainProgram.GRIDSIZE; column++)
					scoreOfGrids[row, column] = calculateScoreOfGrid(grids, row, column);
			}
			
			//最高の評価値のマスを探索する処理
			int maxScore = scoreOfGrids[0, 0];
			rowOfMaxScore = 0;
			columnOfMaxScore = 0;
			for (int row = 0; row < MainProgram.GRIDSIZE; row++)
			{
				for (int column = 0; column < MainProgram.GRIDSIZE; column++)
				{
					if (maxScore < scoreOfGrids[row, column])
					{
						maxScore = scoreOfGrids[row, column];
						rowOfMaxScore = row;
						columnOfMaxScore = column;
					}
				}
			}

			return maxScore;
		}
		
		//指定された方向のひっくり返せる個数を調べる関数
		public static int checkScoreInDirection(int[,] grids, int row, int column, int vecColumn, int vecRow)
		{
			int stone = MainProgram.turnNumber + 1;
			int scoreInDirection = 0;
			row += vecRow;
			column += vecColumn;

			while (grids[row, column] != stone)
			{
				scoreInDirection++;
				row += vecRow;
				column += vecColumn;
			}
				
			return scoreInDirection;
		}
		
		//そこに置いてひっくり返せるのか確認する関数
		public static bool canPutDown(int[,] grids, int row, int column)
		{
			//そもそも空きマスでないと置けない
			if (grids[row, column] != 0)
				return false;
			// 8方向のうち一箇所でもひっくり返せればこの場所に打てる
			// ひっくり返せるかどうかはもう1つのcanPutDownで調べる
			if (canPutDown(grids, row, column, 1, 0))
				return true; // 右
			if (canPutDown(grids, row, column, 0, 1))
				return true; // 下
			if (canPutDown(grids, row, column, -1, 0))
				return true; // 左
			if (canPutDown(grids, row, column, 0, -1))
				return true; // 上
			if (canPutDown(grids, row, column, 1, 1))
				return true; // 右下
			if (canPutDown(grids, row, column, -1, -1))
				return true; // 左上
			if (canPutDown(grids, row, column, 1, -1))
				return true; // 右上
			if (canPutDown(grids, row, column, -1, 1))
				return true; // 左下

			// どの方向もだめな場合はここには打てない
			return false;
		}
		
		//指定方向の石がひっくり返せるか確認する関数
		public static bool canPutDown(int[,] grids, int row, int column, int vecColumn, int vecRow)
		{
			int stone = MainProgram.turnNumber + 1;

			//隣が盤面内か、自分の石でないか、空白でないかチェック
			row += vecRow;
			column += vecColumn;
			//インデックスが0から始まるため-1している
			if (row < 0 || column < 0 || row > MainProgram.GRIDSIZE - 1 || column > MainProgram.GRIDSIZE - 1)
				return false;
			if (grids[row, column] == stone)
				return false;
			if (grids[row, column] == 0)
				return false;
			
			//さらにその隣を調べていく
			column += vecColumn;
			row += vecRow;
			while (row >= 0 && column >= 0　&& row < MainProgram.GRIDSIZE && column < MainProgram.GRIDSIZE)
			{
				if (grids[row, column] == 0) 
					return false;
				if (grids[row, column] == stone)
					return true;
				row += vecRow;
				column += vecColumn;
			}

			return false; //探索の結果、相手の石しか無いときはひっくり返せない
		}
		
		//ひっくり返す関数
		public static int[,] reverse(int[,] grids, int row, int column, int vecColumn, int vecRow)
		{
			int stone = MainProgram.turnNumber + 1;
			row += vecRow;
			column += vecColumn;

			while (grids[row, column] != stone)
			{
				grids[row, column] = stone;
				row += vecRow;
				column += vecColumn;
			}

			return grids;
		}
		
		//盤面を描写する関数
		public static void printGrid(int[,] grids)
		{
			Console.WriteLine("    a b c d e f g h"); //列番号
			for (int row = 0; row < MainProgram.GRIDSIZE; row++)
			{
				Console.Write(row + 1 + "   "); //行番号
				
				for (int column = 0; column < MainProgram.GRIDSIZE; column++)
					Console.Write(grids[row, column] + " ");
				
				Console.WriteLine("   " + (row + 1));
			}
		}
		
		//row、columnを適当な文字列に整形してくれる関数
		public static string shapingNumber(int row, int column)
		{
			string str = "";
			
			if (column == 0) str += "a";
			else if (column == 1) str += "b";
			else if (column == 2) str += "c";
			else if (column == 3) str += "d";
			else if (column == 4) str += "e";
			else if (column == 5) str += "f";
			else if (column == 6) str += "g";
			else if (column == 7) str += "h";
			str += row + 1; //インデックスが1から始まるため+1
			
			return str;
		}
		
		//上の関数とは逆に、文字列を適当なrow,columnに直してくれる関数
		public static void shapingStr(string nextHand, out int row, out int column)
		{
			//手番を数字のインデックスに直す処理
			string s1 = nextHand.Substring(0, 1);
			string s2 = nextHand.Substring(1, 1);
			row = int.Parse(s2) - 1; //インデックスが1から始まるため-1
			column = 0;
			
			if (s1 == "a") column = 0;
			else if (s1 == "b") column = 1;
			else if (s1 == "c") column = 2;
			else if (s1 == "d") column = 3;
			else if (s1 == "e") column = 4;
			else if (s1 == "f") column = 5;
			else if (s1 == "g") column = 6;
			else if (s1 == "h") column = 7;
		}
		
		//二次元配列をコピーする関数
		public static int[,] copyArray(int[,] array)
		{
			int[,] copyArr = new int[MainProgram.GRIDSIZE, MainProgram.GRIDSIZE];
			for (int row = 0; row < MainProgram.GRIDSIZE; row++)
			{
				for (int column = 0; column < MainProgram.GRIDSIZE; column++)
					copyArr[row, column] = array[row, column];
			}
			
			return copyArr;
		}
	}
}
