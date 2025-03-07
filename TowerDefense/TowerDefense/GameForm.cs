﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;

namespace TowerDefense
{
    
    public partial class GameForm : Form
    {
        private Game game;
        private Level level;
        private Image gameSceneImage;

        public GameForm()
        {
            this.Padding = new Padding(GridParams.PaddingSize, GridParams.PaddingSize, GridParams.PaddingSize, GridParams.PaddingSize);
            gameSceneImage = new Bitmap(GridParams.GridSizeX * GridParams.TileSize, GridParams.GridSizeY * GridParams.TileSize, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            InitializeComponent();
            Size panelSize = new Size(GridParams.StartX+GridParams.TileSize*GridParams.GridSizeX, GridParams.StartY + GridParams.TileSize * GridParams.GridSizeY);

            //设置父组件为GameForm窗体
            help_panel.Parent = start_menu_panel.Parent;
            help_panel.Size = panelSize;
            game_scene_panel.Parent = start_menu_panel.Parent;
            game_scene_panel.Size = panelSize;
            start_menu_panel.Size = panelSize;
            cover_pictureBox.Size = panelSize;
            this.Size = panelSize;

            game_scene_panel.Location = new Point(0, 0);
            start_menu_panel.Location = new Point(0, 0);
            help_panel.Location = new Point(0, 0);

            //添加子组件到start_menu_penel
            start_menu_panel.Controls.Add(cover_pictureBox);
            cover_pictureBox.Controls.Add(start_game_button);
            cover_pictureBox.Controls.Add(exit_button);
            cover_pictureBox.Controls.Add(select_level_button);
            cover_pictureBox.Controls.Add(help_button);

            //添加子组件到help_panel
            help_panel.Controls.Add(help_content_textBox);
            
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Console.WriteLine("start game button!");
            //start_menu_panel.Parent.Controls.Remove(start_menu_panel);
            //start_menu_panel.Dispose();
            start_menu_panel.Visible = false;

            game = new Game("chapter1/1-1.txt");
            level = game.Level;

            //timer1.Start();

            Thread thread = new Thread(() => { game.waveRun(this); });
            thread.Start();

            //加载游戏界面的panel
            game_scene_panel.Visible = true;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Console.WriteLine("select level button!");
            //start_menu_panel.Parent.Controls.Remove(start_menu_panel);
            //start_menu_panel.Dispose();
            start_menu_panel.Visible = false;
            //加载选关页面的panel
        }

        private void exit_button_Click(object sender, EventArgs e)
        {
            Console.WriteLine("exit button!");
            Application.Exit();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void help_button_Click(object sender, EventArgs e)
        {
            start_menu_panel.Visible = false;
            help_panel.Visible = true;
            string configFile = @"resource/games_config/help.txt"; // 替换为你的配置文件路径

            using (StreamReader sr = new StreamReader(configFile))
            {
                string content = sr.ReadToEnd();
                help_content_textBox.Text = content;
                
            }
        }

        private void help_to_start_menu_button_Click(object sender, EventArgs e)
        {
            help_panel.Visible = false;
            start_menu_panel.Visible = true;
        }

        private void game_scene_panel_Paint(object sender, PaintEventArgs e)
        {
            // 将游戏场景绘制到一张图片上
            Graphics sceneG = Graphics.FromImage(gameSceneImage);

            // 记录路径
            bool[,] roadMark = new bool[GridParams.GridSizeX, GridParams.GridSizeY];

            // 绘制起点终点
            if (level.path.Count > 0)
            {
                roadMark[level.path.First().x, level.path.First().y] = true;
                sceneG.DrawImage(Resource1.entrance, GridParams.TileSize * level.path.First().x, GridParams.TileSize * level.path.First().y, GridParams.TileSize, GridParams.TileSize);

                roadMark[level.path.Last().x, level.path.Last().y] = true;
                sceneG.DrawImage(Resource1.exit, GridParams.TileSize * level.path.Last().x, GridParams.TileSize * level.path.Last().y, GridParams.TileSize, GridParams.TileSize);
            }

            // 绘制路径
            for (int i= 1;i < level.path.Count() - 1;++i)
            {
                var tile = level.path[i];
                roadMark[tile.x, tile.y] = true;
                sceneG.DrawImage(Resource1.road, GridParams.TileSize * tile.x, GridParams.TileSize * tile.y, GridParams.TileSize, GridParams.TileSize);
            }

            // 绘制其他网格
            for (int i = 0; i < GridParams.GridSizeX; ++i)
            {
                for (int j = 0; j < GridParams.GridSizeY; ++j)
                {
                    if (!roadMark[i, j])
                    {
                        sceneG.DrawImage(Resource1.tile, GridParams.TileSize * i, GridParams.TileSize * j, GridParams.TileSize, GridParams.TileSize);
                    }
                }
            }

            // 绘制塔与敌人
            game.paint(sceneG);

            // 将场景图片绘制到屏幕上
            Graphics g = e.Graphics;
            g.DrawImage(gameSceneImage, GridParams.StartX, GridParams.StartY, GridParams.GridSizeX * GridParams.TileSize, GridParams.GridSizeY * GridParams.TileSize);
            
            List<GameCharacter> characters = new List<GameCharacter>(); // 获取你的角色列表
            GameCharacter character1 = new GameCharacter();
            GameCharacter character2 = new GameCharacter();
            GameCharacter character3 = new GameCharacter();
            character1.Icon = Resource1.tile;
            character2.Icon = Resource1.road;
            character3.Icon = Resource1.tower;
            character1.Name = "character 1";
            character2.Name = "character 2";
            character3.Name = "character 3";
            characters.Add(character1);
            characters.Add(character2);
            characters.Add(character3);

            int startY = 0;
            int startX = 0;
            int padding = 10; // 间隔大小
            int itemHeight = 50 + 2 * padding; // 图像高度 + 上下间隔
            int itemWidth = 160; // 宽度设定为160像素

            Panel panel = new Panel();
            panel.Size = new Size(itemWidth, this.Height - 120); // 假设你的窗体高度足够大
            panel.Location = new Point(0, 120);
            panel.AutoScroll = true; // 如果内容超出Panel的大小，则自动显示滚动条

            // 遍历每一个角色，创建PictureBox和Label，并将它们添加到窗体中
            foreach (var character in characters)
            {
                // 创建并设置Label
                Label lbl = new Label();
                lbl.Text = character.Name;
                lbl.AutoSize = false; // 不自动调整大小，将其设定为指定宽度
                lbl.Width = itemWidth - 50 - 3 * padding; // 预留出图片和额外的padding的空间
                lbl.Location = new Point(startX + padding, startY + padding + (50 - lbl.Height) / 2); // 使得label和图片在同一行中居中
                lbl.Click += Item_Click; // 添加点击事件处理器

                // 创建并设置PictureBox
                PictureBox pb = new PictureBox();
                pb.Name=character.Name;
                pb.Image = character.Icon;
                pb.SizeMode = PictureBoxSizeMode.StretchImage;
                pb.Height = 50;
                pb.Width = 50;
                pb.Location = new Point(lbl.Width + 2 * padding, startY + padding); // 在label的右边绘制图片
                pb.Click += Item_Click; // 添加点击事件处理器

                // 将PictureBox和Label添加到Panel中
                panel.Controls.Add(lbl);
                panel.Controls.Add(pb);

                // 更新下一个item的开始Y坐标
                startY += itemHeight;
            }
            // 将Panel添加到窗体中
            game_scene_panel.Controls.Add(panel);
            
            this.Invalidate();

            game_scene_panel.Invalidate();
        }
        private void Item_Click(object sender, EventArgs e)
        {
            if (sender is Label lbl)
            {
                // 如果点击的是Label，你可以通过lbl.Text获取角色名称
                MessageBox.Show("You clicked on " + lbl.Text);
            }
            else if (sender is PictureBox pb)
            {
                // 如果点击的是PictureBox，Name获取名称
                MessageBox.Show("You clicked on " + pb.Name);
            }
        }

        public void waveCallback(int val)
        {
            //callback: 0:failed, 1:wave success, 2:level complete
            throw new NotImplementedException();
            //TODO
        }

    }
    public class GameCharacter
    {
        public Image Icon { get; set; }
        public string Name { get; set; }
    }
}
