using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        int userID = 0;
        string connectionString = ConfigurationManager.ConnectionStrings["myConnection"].ConnectionString;

        public Form1()
        {
            InitializeComponent();
        }

        

        private void firstName_Click(object sender, EventArgs e)
        {

        }

        private void LoadUser()
        {
            //string connectionString = ConfigurationManager.ConnectionStrings["myConnection"].ConnectionString;
            SqlConnection connection = new SqlConnection(connectionString);

            string sql = "SELECT *FROM UserTable";

            SqlCommand cmd = new SqlCommand(); //we can declare SqlCommand in this method as well as below in btnSave
            cmd.CommandText = sql;
            cmd.Connection = connection;   
            
            //We dont have to search, insert, update or delete here as we're just pulling data from DB
            //so no need to do/add the parameters step

            //Opening connection
            connection.Open();

            //DB bata lyako data haru adapter through, either Data Table or Data Set ma rakhchau
            SqlDataAdapter adapter = new SqlDataAdapter(cmd); //sabai kaam sql cmd ma garisakyo so passing cmd object

            //DataTable: Single Table (euta table ko matra data aucha)
            //DataSet: Multiple Tables

            //Since we're only using data from one table that is UserTable, we use DataTable
            DataTable dt = new DataTable(); //we take value for DataTable from adapter

            //adapter bata ako data, DataTable ko object ma fill garyo
            adapter.Fill(dt);
            grdUser.AutoGenerateColumns = false;
            grdUser.DataSource = dt; //data will bind in the Data Grid in the form

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //Step 1: Get Controls
            string firstName = txtFirstName.Text;
            string lastName = txtLastName.Text;
            string email = txtEmail.Text;
            string address = txtAddress.Text;
            string role = ddlUserRole.Text;

            if (IfAlreadyExists(email))
            {
                MessageBox.Show("User already exists");
            }
            else
            {
                //Step 2: Connection String
                //string connectionString = ConfigurationManager.ConnectionStrings["myConnection"].ConnectionString;
                SqlConnection sqlConnection = new SqlConnection(connectionString);
                string sql = @"INSERT INTO UserTable(FirstName, LastName, Email, Address, UserRole, CreatedBy, CreatedDate) 
                           VALUES(@FirstName, @LastName, @Email, @Address, @UserRole, @CreatedBy, @CreatedDate)";

                SqlCommand cmd = new SqlCommand(sql, sqlConnection);


                //Step 3: Pass Parameters
                cmd.Parameters.AddWithValue("@FirstName", firstName);  //Here firstname is the string we defined earlier
                cmd.Parameters.AddWithValue("@LastName", lastName);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Address", address);
                cmd.Parameters.AddWithValue("@UserRole", role);
                cmd.Parameters.AddWithValue("@CreatedBy", 2); //since we don't have a createdBy defined we will keep a static value
                cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);


                //Step 4: Open Connection
                sqlConnection.Open();

                //Step 5: Execute Command
                int result = cmd.ExecuteNonQuery(); //Used when we need to Insert, Delete, Update

                //Step 6: Close Connection
                sqlConnection.Close();

                if (result > 0)
                {
                    MessageBox.Show("New User inserted successfully");
                    LoadUser();
                    ResetControls();
                }
                else
                {
                    MessageBox.Show("Failed to insert record");
                }
            }

        }

        //To avoid duplicate emails
        private bool IfAlreadyExists(string email)
        {
            SqlConnection sqlConnection = new SqlConnection(connectionString);

            string sql = @"SELECT UserID FROM UserTable WHERE Email = @Email";

            SqlCommand cmd = new SqlCommand(sql, sqlConnection);

            //Step 3: Pass parameter
            cmd.Parameters.AddWithValue("@Email", email);

            sqlConnection.Open();
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                return true;
            }
            return false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ddlUserRole.SelectedIndex = 0;
            ddlSearchByRole.SelectedIndex = 0;
            btnUpdate.Visible = false;
            LoadUser();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            ResetControls();
        }

        private void ResetControls()
        {
            txtFirstName.Text = string.Empty;
            txtLastName.Text = string.Empty;
            txtEmail.Text = string.Empty;
            txtAddress.Text = string.Empty;
            ddlUserRole.SelectedIndex = 0;
            ddlSearchByRole.SelectedIndex = 0;
            userID = 0; //if we don't declare there will be garbage values

            btnUpdate.Visible = false;
            btnSave.Visible = true;
        }

        private void srchRole_Click(object sender, EventArgs e)
        {

        }

        private void urole_Click(object sender, EventArgs e)
        {

        }

        private void ddlSearchByRole_SelectedIndexChanged(object sender, EventArgs e)
        {
            string userRole = ddlSearchByRole.SelectedItem.ToString();
            if (userRole.Contains("Select"))
            {
                LoadUser();
            }
            else
            {
                //string connectionString = ConfigurationManager.ConnectionStrings["myConnection"].ConnectionString;
                SqlConnection sqlConnection = new SqlConnection(connectionString);

                string sql = "SELECT *FROM UserTable WHERE UserRole LIKE '%" + userRole + "%'";
                SqlCommand cmd = new SqlCommand(sql, sqlConnection);

                sqlConnection.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);

                grdUser.AutoGenerateColumns = false;
                grdUser.DataSource = dataTable;
            }



            
        }

        private void grdUser_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            string headerText = grdUser.Columns[e.ColumnIndex].HeaderText;
            if (headerText == "Delete")
            {
                var confirmationResult = MessageBox.Show("Are you sure want to delete?", "Delete Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (confirmationResult == DialogResult.Yes)
                {
                    userID = Convert.ToInt32(grdUser.Rows[e.RowIndex].Cells[0].Value.ToString()); //userID ma value leko
                    DeleteUser();
                }
            }
            else if (headerText == "Edit")
            {
                EditRecord(e);
            }


        }

        private void EditRecord(DataGridViewCellEventArgs e)
        {
            var row = grdUser.Rows[e.RowIndex]; //declaring cause it is repetitive
            userID = Convert.ToInt32(grdUser.Rows[e.RowIndex].Cells[0].Value.ToString());

            //to populate First, last name, email, address
            txtFirstName.Text = row.Cells["FirstName"].Value.ToString();
            txtLastName.Text = row.Cells["LastName"].Value.ToString();
            txtEmail.Text = row.Cells["Email"].Value.ToString();
            txtAddress.Text = row.Cells["Address"].Value.ToString();

            //for populating the User Role
            ddlUserRole.SelectedItem = row.Cells["UserRole"].Value.ToString();

            btnUpdate.Visible = true;
            btnSave.Visible = false;
        }

        private void DeleteUser()
        {
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand("DELETE FROM UserTable WHERE UserID = @UserID", sqlConnection);

            cmd.Parameters.AddWithValue("@UserID", userID); //UserID field ma Parameter pass gareko

            sqlConnection.Open();
            int result = cmd.ExecuteNonQuery();
            if (result > 0)
            {
                LoadUser(); //calling to re-bind grid
                MessageBox.Show("User deleted successfully", "Notification");

            }
            else
            {
                MessageBox.Show("Failed to delete record", "Notification");
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (userID>0)
            {
                string fName = txtFirstName.Text;
                string lName = txtLastName.Text;
                string email = txtEmail.Text;
                string addr = txtAddress.Text;
                string userRole = ddlUserRole.SelectedItem.ToString();

                SqlConnection con = new SqlConnection(connectionString);

                string sqlQuery = "UPDATE UserTable SET FirstName = @FirstName, LastName = @LastName, Email = @Email, Address = @Address, UserRole = @UserRole, ModifiedBy = 1, ModifiedDate = @ModifiedDate WHERE UserID=@UserID";

                SqlCommand cmd = new SqlCommand(sqlQuery, con);
                cmd.Parameters.AddWithValue("@FirstName", fName);
                cmd.Parameters.AddWithValue("@LastName", lName);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Address", addr);
                cmd.Parameters.AddWithValue("@UserRole", userRole);
                cmd.Parameters.AddWithValue("@UserID", userID);
                cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);

                con.Open();
                int result = cmd.ExecuteNonQuery();
                if (result > 0)
                {
                    MessageBox.Show("Record updated successfully", "Information");
                    LoadUser();
                    ResetControls();
                }
                else
                {
                    MessageBox.Show("Failed to update record");

                }


            }
            

        }
    }
}
