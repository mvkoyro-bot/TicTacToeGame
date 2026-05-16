using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace TicTacToeGame
{
    public class GameData
    {
        public string[] Board { get; set; } = new string[9];
        public string CurrentPlayer { get; set; } = "X";
        public int ScoreX { get; set; }
        public int ScoreO { get; set; }
        public int ScoreDraw { get; set; }
        public bool GameOver { get; set; }
        public string Winner { get; set; } = "";
    }

    public class Form1 : Form
    {
        private Button[,] buttons = new Button[3, 3];
        private string currentPlayer = "X";
        private bool gameOver = false;
        private int scoreX = 0;
        private int scoreO = 0;
        private int scoreDraw = 0;
        private Label lblStatus;
        private Label lblScore;
        private Button btnSave;
        private Button btnLoad;
        private Button btnReset;
        private string saveFilePath = "game_save.json";

        public Form1()
        {
            SetupGame();
        }

        private void SetupGame()
        {
            this.Text = "Крестики-нолики";
            this.Size = new Size(350, 470);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition = FormStartPosition.CenterScreen;

            TableLayoutPanel table = new TableLayoutPanel();
            table.Size = new Size(300, 300);
            table.Location = new Point(25, 25);
            table.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            table.RowCount = 3;
            table.ColumnCount = 3;

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    Button btn = new Button();
                    btn.Font = new Font("Arial", 24, FontStyle.Bold);
                    btn.Dock = DockStyle.Fill;
                    btn.Tag = new Point(i, j);
                    btn.Click += CellClick;
                    buttons[i, j] = btn;
                    table.Controls.Add(btn, j, i);
                }

            lblStatus = new Label() { Text = "Ход игрока X", Font = new Font("Arial", 12), Size = new Size(200, 30), Location = new Point(25, 340) };
            lblScore = new Label() { Text = "Счёт: X - 0 : O - 0 : Ничьи - 0", Font = new Font("Arial", 10), Size = new Size(250, 30), Location = new Point(25, 370) };
            btnSave = new Button() { Text = "Сохранить", Location = new Point(25, 410), Size = new Size(80, 30) };
            btnLoad = new Button() { Text = "Загрузить", Location = new Point(115, 410), Size = new Size(80, 30) };
            btnReset = new Button() { Text = "Новая игра", Location = new Point(205, 410), Size = new Size(95, 30) };

            btnSave.Click += BtnSave_Click;
            btnLoad.Click += BtnLoad_Click;
            btnReset.Click += BtnReset_Click;

            this.Controls.Add(table);
            this.Controls.Add(lblStatus);
            this.Controls.Add(lblScore);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnLoad);
            this.Controls.Add(btnReset);
        }

        private void CellClick(object sender, EventArgs e)
        {
            if (gameOver) return;
            Button btn = (Button)sender;
            if (btn.Text != "") return;

            btn.Text = currentPlayer;
            btn.ForeColor = (currentPlayer == "X") ? Color.Green : Color.Red;

            if (CheckWin())
            {
                gameOver = true;
                if (currentPlayer == "X") scoreX++; else scoreO++;
                lblStatus.Text = (currentPlayer == "X") ? "Победил X!" : "Победил O!";
                UpdateScoreLabel();
                return;
            }
            if (IsDraw())
            {
                gameOver = true;
                scoreDraw++;
                lblStatus.Text = "Ничья!";
                UpdateScoreLabel();
                return;
            }
            currentPlayer = (currentPlayer == "X") ? "O" : "X";
            lblStatus.Text = $"Ход игрока {currentPlayer}";
        }

        private bool CheckWin()
        {
            for (int i = 0; i < 3; i++)
                if (buttons[i, 0].Text == currentPlayer && buttons[i, 1].Text == currentPlayer && buttons[i, 2].Text == currentPlayer) return true;
            for (int i = 0; i < 3; i++)
                if (buttons[0, i].Text == currentPlayer && buttons[1, i].Text == currentPlayer && buttons[2, i].Text == currentPlayer) return true;
            if (buttons[0, 0].Text == currentPlayer && buttons[1, 1].Text == currentPlayer && buttons[2, 2].Text == currentPlayer) return true;
            if (buttons[0, 2].Text == currentPlayer && buttons[1, 1].Text == currentPlayer && buttons[2, 0].Text == currentPlayer) return true;
            return false;
        }

        private bool IsDraw()
        {
            foreach (Button btn in buttons) if (btn.Text == "") return false;
            return true;
        }

        private void UpdateScoreLabel() => lblScore.Text = $"Счёт: X - {scoreX} : O - {scoreO} : Ничьи - {scoreDraw}";

        private void ResetGame(bool resetScore = false)
        {
            foreach (Button btn in buttons) btn.Text = "";
            currentPlayer = "X";
            gameOver = false;
            lblStatus.Text = "Ход игрока X";
            if (resetScore) { scoreX = 0; scoreO = 0; scoreDraw = 0; UpdateScoreLabel(); }
        }

        private void BtnReset_Click(object sender, EventArgs e) => ResetGame(false);

        private void BtnSave_Click(object sender, EventArgs e)
        {
            var data = new GameData();
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    data.Board[i * 3 + j] = buttons[i, j].Text;
            data.CurrentPlayer = currentPlayer;
            data.ScoreX = scoreX;
            data.ScoreO = scoreO;
            data.ScoreDraw = scoreDraw;
            data.GameOver = gameOver;
            data.Winner = gameOver ? (CheckWin() ? currentPlayer : "") : "";
            File.WriteAllText(saveFilePath, JsonSerializer.Serialize(data));
            MessageBox.Show("Игра сохранена!", "Сохранение", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            if (!File.Exists(saveFilePath))
            {
                MessageBox.Show("Файл сохранения не найден.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var data = JsonSerializer.Deserialize<GameData>(File.ReadAllText(saveFilePath));
            if (data == null) return;
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    buttons[i, j].Text = data.Board[i * 3 + j];
            currentPlayer = data.CurrentPlayer;
            scoreX = data.ScoreX;
            scoreO = data.ScoreO;
            scoreDraw = data.ScoreDraw;
            gameOver = data.GameOver;
            UpdateScoreLabel();
            lblStatus.Text = gameOver ? (data.Winner == "X" ? "Победил X!" : data.Winner == "O" ? "Победил O!" : "Ничья!") : $"Ход игрока {currentPlayer}";
            MessageBox.Show("Игра загружена!", "Загрузка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}