﻿using FootballFieldManagement.DAL;
using FootballFieldManagement.Models;
using FootballFieldManagement.ViewModels;
using FootballFieldManagement.Views;
using FootballFieldManegement.DAL;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FootballFieldManagement.ViewModels
{
    class LoginViewModel : BaseViewModel
    {
        public ICommand LogInCommand { get; set; }
        public ICommand OpenSignUpWindowCommand { get; set; }
        public ICommand PasswordChangedCommand { get; set; }
        public ICommand OpenCheckAttendanceWindowCommand { get; set; }
        private string password;
        public string Password { get => password; set { password = value; OnPropertyChanged(); } }
        private string userName;
        public string UserName { get => userName; set { userName = value; OnPropertyChanged(); } }
        private bool isLogin;
        public bool IsLogin { get => isLogin; set => isLogin = value; }
        public Employee employee;
        public LoginViewModel()
        {
            LogInCommand = new RelayCommand<LoginWindow>((parameter) => true, (parameter) => Login(parameter));
            PasswordChangedCommand = new RelayCommand<PasswordBox>((parameter) => true, (parameter) => EncodingPassword(parameter));
            OpenSignUpWindowCommand = new RelayCommand<Window>((parameter) => true, (parameter) => OpenSignUpWindow(parameter));
            OpenCheckAttendanceWindowCommand = new RelayCommand<Window>((parameter) => true, (parameter) => OpenCheckAttendanceWindow(parameter));
        }
        public void OpenCheckAttendanceWindow(Window parameter)
        {
            CheckAttendanceWindow wdCheckAttendance = new CheckAttendanceWindow();
            parameter.Hide();
            wdCheckAttendance.ShowDialog();
            parameter.Show();
        }
        public void Login(LoginWindow parameter)
        {
            isLogin = false;
            if (parameter == null)
            {
                return;
            }
            List<Account> accounts = AccountDAL.Instance.ConvertDBToList();
            //check username
            if (string.IsNullOrEmpty(parameter.txtUsername.Text))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập!");
                parameter.txtUsername.Focus();
                return;
            }
            //check password
            if (string.IsNullOrEmpty(parameter.txtPassword.Password))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu!");
                parameter.txtPassword.Focus();
                return;
            }
            foreach (var account in accounts)
            {
                if (account.Username == parameter.txtUsername.Text.ToString() && account.Password == password)
                {
                    CurrentAccount.Type = account.Type == 1 ? true : false; // Kiểm tra quyền
                    List<Employee> employees = EmployeeDAL.Instance.ConvertDBToList();
                    foreach (var employee in employees)
                    {
                        if (employee.IdAccount == account.IdAccount)
                        {
                            //Lấy thông tin người đăng nhập
                            CurrentAccount.DisplayName = employee.Name;
                            CurrentAccount.Image = employee.ImageFile;
                            CurrentAccount.IdAccount = employee.IdAccount;
                            CurrentAccount.Password = password;
                            this.employee = employee;
                            break;
                        }
                    }
                    isLogin = true;
                }
            }
            if (isLogin)
            {
                HomeWindow home = new HomeWindow();
                home.lbTitle.Content = new DataProvider().LoadData("FieldName").Rows[0].ItemArray[0].ToString();
                SetJurisdiction(home);
                DisplayAccount(home);
                DisplayEmployee(employee, home);
                parameter.Hide();
                home.ShowDialog();
                parameter.txtPassword.Password = null;  
                parameter.Show();
            }
            else
            {
                MessageBox.Show("Tên đăng nhập hoặc mật khẩu không chính xác!");
            }
        }
        public void DisplayEmployee(Employee employee,HomeWindow home)
        {
            if (CurrentAccount.Type)
            {
                home.txtIDEmployee.Text = employee.IdEmployee.ToString();
                home.txtName.Text = employee.Name;
                home.txtPosition.Text = employee.Position;
                home.txtDayOfBirth.Text = employee.DateOfBirth.ToShortDateString();
                home.txtGender.Text = employee.Gender;
                home.txtAddress.Text = employee.Address;
                home.txtPhoneNumber.Text = employee.Phonenumber;
                ImageBrush imageBrush = new ImageBrush();
                BitmapImage bitmapImage = Converter.Instance.ConvertByteToBitmapImage(CurrentAccount.Image);
                imageBrush.ImageSource = bitmapImage;
                if (bitmapImage != null)
                    home.grdImageEmployee.Background = imageBrush; // Hiển thị hình ảnh 
            }
            else
            {
                home.txtIDEmployee.Text = 0.ToString();
                home.txtName.Text = "Chủ sân";
                home.txtPosition.Text = "Chủ sân";
                home.txtDayOfBirth.IsEnabled = false;
                home.txtGender.IsEnabled = false;
                home.txtAddress.IsEnabled = false;
                home.txtPhoneNumber.IsEnabled = false;
            }    
        }
        public void SetJurisdiction(HomeWindow home)
        {
            if (CurrentAccount.Type)
            {
                //Không cấp quyền cho nhân viên
                home.stkMenu.Children.Remove(home.stkMenu.Children[0]);
                home.txtNewFieldName.IsEnabled = false;
                home.btnEmployee.IsEnabled = false;
                home.btnReport.IsEnabled = false;
                home.btnAddGoods.IsEnabled = false;
                home.btnAddEmployee.IsEnabled = false;
                home.btnSetSalary.IsEnabled = false;
                home.icnEmployee.Foreground = Brushes.LightGray;
                home.icnReport.Foreground = Brushes.LightGray;
            }
        }
        public void DisplayAccount(HomeWindow home)
        {      
            if (CurrentAccount.Type==true)
            {
                home.lbAccount.Content = CurrentAccount.DisplayName;// Hiển thị tên nhân viên
                ImageBrush imageBrush = new ImageBrush();
                BitmapImage bitmapImage = Converter.Instance.ConvertByteToBitmapImage(CurrentAccount.Image);
                imageBrush.ImageSource = bitmapImage;
                if(bitmapImage!=null)
                    home.imgAccount.Fill = imageBrush; // Hiển thị hình ảnh 
            }
        }
        public void OpenSignUpWindow(Window parameter)
        {
            SignUpWindow signUp = new SignUpWindow();
            parameter.Hide();
            signUp.ShowDialog();
            parameter.Show();
        }

        public void EncodingPassword(PasswordBox parameter)
        {
            this.password = parameter.Password;
            this.password = MD5Hash(this.password);
        }

    }
}
