﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Test_Rating.Model;

namespace Test_Rating.SVDPP
{

    public class SVDPP
    {

        private static int UserCount = 100;                                     //Quantidade de Usuarios.
        private static int AdvertisementCount = 10;                             //Quantidade de Anuncios
        private static int BestAdvertisement = 10;   
        private static int NumberLatentFactors = 9;                             //Quantidade de fatores.
        private static double TrainingSpeed = 0.025;

        private static double Coefficient1 = 0.0005;                            // Coeficiente de regularização 1
        private static double Coefficient2 = 0.0025;                            // Coeficiente de regularização 2
        private static double CoefficientError = 0.00001;                       // Coeficiente de precisão de erro de erro
        private static double CoefficientThreshold = 0.01;                      // Coeficiente de limiar

        private static List<double> UserPredictors = new List<double>();                // Declarando o vetor de preditores de linha de base de usuários        
        private static List<double> ItemPredictors = new List<double>();                // Declarando o vetor de preditores de linha de base de itens
        private static List<List<double>> UserLatent = new List<List<double>>();        // Declarando a matriz dos fatores latentes do usuário
        private static List<List<double>> ItemLatent = new List<List<double>>();        // Declarando a matriz dos fatores latentes do item
        private static List<List<double>> MatrixRatings = new List<List<double>>();     // Declarando a matriz de avaliações

        private static List<UserAdvertisement> listUserAdvertisement = new List<UserAdvertisement>(); //Avaliações dos usuarios com os ratings.

        
        public static StringBuilder BestMatrix = new StringBuilder();       //Best Recomend

        public static StringBuilder ReturnMatrix = new StringBuilder();     //Retorno Html.

        public static StringBuilder OrigemMatrix = new StringBuilder();     //Matrix Original.
        
        private static readonly Random random = new Random();               // Número aleatorio.
        private static readonly object syncLock = new object();


        public static void Main1()
        {

            // Posso utilizar outros criterios.
            // 
            // Entrou em contato Telefone = 5
            // Entrou em contato E-mail = 4
            // Favoritos = 3
            // Ainda não viu o anuncio = 2
            // Anuncio Antigo =1 (Superior a 3 meses.)


            GenerateRating();           
            
            Initialize();               

            Learn();                    

            Predict();                  

            ConvertMatrix();            

            ShowResult2();              // Antes e o Depois.

            BestRating();

        }


        /// <summary>
        /// Gerador de dados para o Rating.
        /// </summary>
        private static void GenerateRating()
        {

            listUserAdvertisement = new List<UserAdvertisement>();

            var id = 1;

            //100 Usuarios.
            for (int i = 1; i <= UserCount; i++)
            {

                // 1 Usuario avaliou todos os anuncios ativos.
                // Caso não tenha avaliado será 0.
                for (int k = 1; k <= AdvertisementCount; k++)
                {
                    var UserAdvertisement = new UserAdvertisement();

                    UserAdvertisement.User = new User();
                    UserAdvertisement.User.UserId = i;
                    UserAdvertisement.User.Name = "User " + i;

                    UserAdvertisement.Advertisement = new Advertisement();
                    UserAdvertisement.Advertisement.Id = k;
                    UserAdvertisement.Advertisement.Description = "Anuncio " + k;

                    UserAdvertisement.Rating = RandomNumber(0, 11);

                    if (i == 1)
                        UserAdvertisement.Rating = 0;

                    UserAdvertisement.UserAdvertisementId = id;
                    id++;

                    listUserAdvertisement.Add(UserAdvertisement);
                }
            }

            var AllUser = listUserAdvertisement
                .Select(x => x)
                .GroupBy(x => x.User.UserId)
                .ToList();

            MatrixRatings = null;
            MatrixRatings = new List<List<double>>();

            var Row = new List<double>();
            foreach (var item in AllUser)
            {
                var ta = listUserAdvertisement.Where(x => x.User.UserId == item.FirstOrDefault().User.UserId).ToList();

                foreach (var item2 in ta)
                {
                    int t = item2.Rating;
                    Row.Add(t);
                }

                MatrixRatings.Add(Row);
                Row = new List<double>();
            }

        }

        /// <summary>
        /// Inicializando o modelo de previsão de classificações 
        /// </summary>
        static void Initialize()
        {
            // Constructing the matrix of user's latent factors by iteratively
            // appending the rows being constructed to the list of rows MF_UserRow
            for (int User = 0; User < MatrixRatings.Count(); User++)
            {
                // Declare a list of items MF_UserRow rated by the current user
                List<double> MF_UserRow = new List<double>();

                // Add the set of elements equal to 0 to the list of items MF_UserRow.
                // The number of elements being added is stored in Factors variable
                MF_UserRow.AddRange(Enumerable.Repeat(0.00, NumberLatentFactors));

                // Append the current row MF_UserRow to the matrix of factors MF_User
                UserLatent.Insert(User, MF_UserRow);
            }

            // Constructing the matrix of item's latent factors by iteratively
            // appending the rows being constructed to the list of rows MF_ItemRow
            for (int Item = 0; Item < MatrixRatings.ElementAt(0).Count(); Item++)
            {
                // Declare a list of items MF_ItemRow rated by the current item
                List<double> MF_ItemRow = new List<double>();
                // Add the set of elements equal to 0 to the list of items MF_ItemRow
                // The number of elements being added is stored in Factors variable
                MF_ItemRow.AddRange(Enumerable.Repeat(0.00, NumberLatentFactors));
                // Append the current row MF_ItemRow to the matrix of factors MF_Item
                ItemLatent.Insert(Item, MF_ItemRow);
            }

            // Intializing the first elements of the matrices of user's 
            // and item's factors with values 0.1 and 0.05
            UserLatent[0][0] = 0.1;
            ItemLatent[0][0] = UserLatent[0][0] / 2;

            // Construct the vector of users baseline predictors by 
            // appending the set of elements equal to 0.The number of elements being 
            // appended is equal to the actual number of rows in the matrix of ratings
            UserPredictors.AddRange(Enumerable.Repeat(0.00, MatrixRatings.Count()));

            // Construct the vector of items baseline predictors by appending
            // the set of elements equal to 0. The number of elements appended 
            // is equal to the actual number of rows in the matrix of ratings
            ItemPredictors.AddRange(Enumerable.Repeat(0.00, MatrixRatings.ElementAt(0).Count()));
        }

        /// <summary>
        /// Treinando o modelo de previsão de classificações
        /// </summary>
        static void Learn()
        {
            // Initializing the iterations loop counter variable
            int Iterations = 0;

            // Initializing the RMSE and RMSE_New variables to store
            // current and previous values of RMSE
            double RMSE = 0.00, RMSE_New = 1.00;

            // Computing the average rating for the entire domain of rated items
            double AvgRating = GetAverageRating(MatrixRatings);

            // Iterating the process of the ratings prediction model update until
            // the value of difference between the current and previous value of RMSE
            // is greater than the value of error precision accuracy EPS (e.g. the learning
            // process has converged).
            while (Math.Abs(RMSE - RMSE_New) > CoefficientError)
            {
                // Assign the previously obtained value of RMSE to the RMSE variable
                // Assign the variable RMSE_New equal to 0
                RMSE = RMSE_New; RMSE_New = 0;

                // Iterate through the matrix of ratings and for each existing rating compute
                // the error value and perform the stochastic gradient descent to update 
                // the main parameters of the ratings prediction model for the current user and item
                for (int User = 0; User < MatrixRatings.Count(); User++)
                {
                    for (int Item = 0; Item < MatrixRatings.ElementAt(0).Count(); Item++)

                        // Perform a check if the current rating in the matrix of ratings is unknown.
                        // If not, perform the following steps to adjust the values of baseline
                        // predictors and factorization vectors
                        if (MatrixRatings[User].ElementAt(Item) > 0)
                        {

                            // Compute the value of estimated rating using formula (2)
                            double Rating = AvgRating + UserPredictors[User] +
                                ItemPredictors[Item] + GetProduct(UserLatent[User], ItemLatent[Item]);

                            // Compute the error value as the difference between the existing and estimated ratings
                            double Error = MatrixRatings[User].ElementAt(Item) - Rating;

                            // Output the current rating given by the current user to the current item
                            Console.Write("{0:0.00}|{1:0.00} ", MatrixRatings[User][Item], Rating);

                            // Add the value of error square to the current value of RMSE
                            RMSE_New = RMSE_New + Math.Pow(Error, 2);

                            // Update the value of average rating for the entire domain of ratings
                            // by performing stochastic gradient descent using formulas (7.1-5)
                            AvgRating = AvgRating + TrainingSpeed * (Error - Coefficient1 * AvgRating);

                            // Update the value of baseline predictor of the current user
                            // by performing stochastic gradient descent using formulas (7.1-5)
                            UserPredictors[User] = UserPredictors[User] + TrainingSpeed * (Error - Coefficient1 * UserPredictors[User]);

                            // Update the value of baseline predictor of the current item 
                            // by performing stochastic gradient descent using formulas (7.1-5)
                            ItemPredictors[Item] = ItemPredictors[Item] + TrainingSpeed * (Error - Coefficient1 * ItemPredictors[Item]);

                            // Update each component of the factorization vector for the current user and item
                            for (int Factor = 0; Factor < NumberLatentFactors; Factor++)
                            {
                                // Adjust the value of the current component of the user's factorization vector 
                                // by performing stochastic gradient descent using formulas (7.1-5)
                                UserLatent[User][Factor] += TrainingSpeed * (Error * ItemLatent[Item][Factor] + Coefficient2 * UserLatent[User][Factor]);
                                // Adjust the value of the current component of the item's factorization vector 
                                // by performing stochastic gradient descent using formulas (7.1-5)
                                ItemLatent[Item][Factor] += TrainingSpeed * (Error * UserLatent[User][Factor] + Coefficient2 * ItemLatent[Item][Factor]);
                            }
                        }

                        // Output the value of unknown rating in the matrix of ratings
                        else Console.Write("{0:0.00}|0.00 ", MatrixRatings[User][Item]);

                    //Console.WriteLine("\n");
                }

                // Compute the current value of RMSE (root means square error)
                RMSE_New = Math.Sqrt(RMSE_New / (MatrixRatings.Count() * MatrixRatings.ElementAt(0).Count()));

                //Console.WriteLine("Iteration: {0}\t RMSE={1}\n\n", Iterations, RMSE_New);

                // Performing a check if the difference between the values 
                // of current and previous values of RMSE exceeds the given threshold
                if (RMSE_New > RMSE - CoefficientThreshold)
                {
                    // If so, reduce the values of training speed and threshold 
                    // by multiplying each value by the value of specific coefficients
                    TrainingSpeed *= 0.66; CoefficientThreshold *= 0.5;
                }

                Iterations++; // Increment the iterations loop counter variable
            }
        }

        /// <summary>
        /// Previsão de classificações para os itens não classificados
        /// </summary>
        public static void Predict()
        {

            // Computing the average rating for the entire domain of rated items
            double AvgRating = GetAverageRating(MatrixRatings);

            //Console.WriteLine("We've predicted the following ratings:\n");
            // Iterating through the MatrixUI matrix of ratings

            for (int User = 0; User < MatrixRatings.Count(); User++)
                for (int Item = 0; Item < MatrixRatings.ElementAt(0).Count(); Item++)

                    // For each rating given to the current item by the current user 
                    // we're performing a check if the current item is unknown
                    if (MatrixRatings[User].ElementAt(Item) == 0)
                    {
                        // If so, compute the rating for the current 
                        // unrated item used baseline estimate formula (2)
                        MatrixRatings[User][Item] = AvgRating + UserPredictors[User] + ItemPredictors[Item] + GetProduct(UserLatent[User], ItemLatent[Item]);

                        // Output the original rating estimated for the current item and the rounded value of the following rating                        
                        //Console.WriteLine("User {0} has rated Item {1} as {2:0.00}|{3:0.00}", User, Item, MatrixUI[User][Item], Math.Round(MatrixUI[User][Item]));
                    }

            //Console.WriteLine();
        }

        /// <summary>
        /// Convert a matrix em classe.  
        /// </summary>
        private static void ConvertMatrix()
        {

            var AllUser = listUserAdvertisement
                .Select(x => x)
                .GroupBy(x => x.User.UserId)
                .ToList();

            int userCount = 0;
            foreach (var item in AllUser)
            {

                var ta = listUserAdvertisement.Where(x => x.User.UserId == item.FirstOrDefault().User.UserId).ToList();

                int AdvCount = 0;
                foreach (var item2 in ta)
                {

                    var r = MatrixRatings[userCount][AdvCount];
                    item2.RatingPredict = (int)r;
                    AdvCount++;
                }

                userCount++;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min">Inclusive</param>
        /// <param name="max">Exclusive</param>
        /// <returns></returns>
        public static int RandomNumber(int min, int max)
        {
            lock (syncLock)
            { // synchronize
                var t = random.Next(min, max);
                if (t <= 0)
                    return 0;

                return t;
            }
        }
       
        public static double GetProduct(List<double> VF_User, List<double> VF_Item)
        {
            // Initialize the variable that is used to 
            // store the inner product of two factorization vectors
            double Product = 0.00;

            // Iterating through the two factorization vectors
            for (int Index = 0; Index < NumberLatentFactors; Index++)
                // Compute the value of product of the two components 
                // of those vectors having the same value of index and 
                // add this value to the value of the variable Product
                Product += VF_User[Index] * VF_Item[Index];

            return Product;
        }
        public static double GetAverageRating(List<List<double>> Matrix)
        {
            // Initialize the variables Sum and Count to store the values of
            // sum of existing ratings in matrix of ratings and the count of
            // existing ratings respectively
            double Sum = 0; int Count = 0;

            // Iterating through the matrix of ratings
            for (int User = 0; User < Matrix.Count(); User++)
                for (int Item = 0; Item < Matrix[User].Count(); Item++)

                    // For each rating performing a check if the current rating is unknown
                    if (Matrix[User][Item] > 0)
                    {
                        // If not, add the value of the current rating to the value of variable Sum
                        Sum = Sum + Matrix[User][Item];
                        // Increment the loop counter variable of existing ratings by 1
                        Count = Count + 1;
                    }

            // Compute and return the value of average 
            // rating for the entire domain of existing ratings
            return Sum / Count;
        }
        //public static void LoadItemsFromFile(string Filename, List<List<double>> Matrix)
        //{

        //    // Intializing the file stream object and open the file
        //    using (System.IO.FileStream fsFile = new System.IO.FileStream(Filename, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
        //    {
        //        // Initializing the stream reader object
        //        using (System.IO.StreamReader fsStream = new System.IO.StreamReader(fsFile, System.Text.Encoding.UTF8, true, 128))
        //        {
        //            string textBuf = "\0";
        //            // Retrieving each line from the file until we reach the end-of-file
        //            while ((textBuf = fsStream.ReadLine()) != null)
        //            {
        //                List<double> Row = new List<double>();
        //                if (!String.IsNullOrEmpty(textBuf))
        //                {
        //                    string sPattern = " ";

        //                    // Iterating through the array of tokens and append each token to the array Row
        //                    foreach (var rating in Regex.Split(textBuf, sPattern))
        //                        Row.Add(double.Parse(rating));
        //                }

        //                // Append the current row to the matrix of ratings
        //                Matrix.Add(Row);
        //            }
        //        }
        //    }
        //}



        private static void BestRating()
        {

            var AllUser = listUserAdvertisement
                .Select(x => x)
                .GroupBy(x => x.User.UserId)                
                .ToList();

            string tab = "\t";

            BestMatrix = null;
            BestMatrix = new StringBuilder();
            BestMatrix.AppendLine("<table class='table'>");

            foreach (var item in AllUser)
            {
                                
                var t = listUserAdvertisement
                    .Select(x=>x)
                    .Where(x => 
                        x.Rating == 0
                        && x.User.UserId == item.FirstOrDefault().User.UserId
                    )
                    .OrderByDescending(x => x.RatingPredict).ThenBy(n => n.ViewAdvertisement)
                    .Take(BestAdvertisement)
                    .ToList();
                BestMatrix.Append("<tr>");

                var showUser = true;

                foreach (var item2 in t)
                    {
                        if(showUser)
                            BestMatrix.AppendFormat("<td>{0}</td>", item2.User.Name);

                        BestMatrix.AppendFormat("<td>{0}</td>", item2.Advertisement.Description );
                        BestMatrix.AppendFormat("<td>{0}-{1}</td>", item2.Rating, item2.RatingPredict);

                        showUser = false;

                    }

                BestMatrix.AppendLine("</tr>");

            }
            BestMatrix.AppendLine("</table>");


        }
        

        private static void WriteMatrix()
        {

            string tab = "\t";

            OrigemMatrix = null;
            OrigemMatrix = new StringBuilder();
            OrigemMatrix.AppendLine("<table>");

            for (int User = 0; User < MatrixRatings.Count(); User++)
            {
                OrigemMatrix.Append(tab + tab + tab + "<tr>");

                for (int Item = 0; Item < MatrixRatings.ElementAt(0).Count(); Item++)
                {
                    string cellValue = Math.Round(MatrixRatings[User][Item]).ToString();

                    if (MatrixRatings[User][Item] > 0)
                    {
                        OrigemMatrix.AppendFormat("<td>{0}</td>", cellValue);
                    }
                    else
                    {
                        //OrigemMatrix.AppendFormat("<td>?</td>");
                    }

                }

                OrigemMatrix.AppendLine("</tr>");
            }
            OrigemMatrix.AppendLine("</table>");

        }

        private static void ShowResult()
        {
            string tab = "\t";

            ReturnMatrix = null;
            ReturnMatrix = new StringBuilder();
            ReturnMatrix.AppendLine("<table>");

            for (int User = 0; User < MatrixRatings.Count(); User++)
            {
                ReturnMatrix.Append(tab + tab + tab + "<tr>");

                for (int Item = 0; Item < MatrixRatings.ElementAt(0).Count(); Item++)
                {
                    string cellValue = Math.Round(MatrixRatings[User][Item]).ToString();
                    ReturnMatrix.AppendFormat("<td>{0}</td>", cellValue);
                }

                ReturnMatrix.AppendLine("</tr>");
            }
            ReturnMatrix.AppendLine("</table>");
        }

        private static void ShowResult2()
        {

            string tab = "\t";

            ReturnMatrix = null;
            ReturnMatrix = new StringBuilder();
            ReturnMatrix.AppendLine("<table class='table'>");

            var AllUser = listUserAdvertisement
                .Select(x => x)
                .GroupBy(x => x.User.UserId)
                .ToList();

            //Header dos produtos.
            ReturnMatrix.Append(tab + tab + tab + "<tr>");
            foreach (var item in AllUser)
            {
                var ta = listUserAdvertisement.Where(x => x.User.UserId == item.FirstOrDefault().User.UserId).ToList();

                ReturnMatrix.AppendFormat("<td>{0}</td>", "---");

                foreach (var item2 in ta)
                {
                    var d = item2.Advertisement.Description;
                    ReturnMatrix.AppendFormat("<td>{0}</td>", d);
                }

                break;
            }

            ReturnMatrix.AppendLine("</tr>");
            //Header dos produtos.

            foreach (var item in AllUser)
            {

                ReturnMatrix.Append(tab + tab + tab + "<tr>");

                var ta = listUserAdvertisement.Where(x => x.User.UserId == item.FirstOrDefault().User.UserId).ToList();

                ReturnMatrix.AppendFormat("<td>{0}</td>", ta.FirstOrDefault().User.Name);

                foreach (var item2 in ta)
                {
                    var t1 = item2.Rating;
                    var t2 = item2.RatingPredict;

                    ReturnMatrix.AppendFormat("<td>({0}-{1})</td>", t1, t2);
                }
                ReturnMatrix.AppendLine("</tr>");
            }

            ReturnMatrix.AppendLine("</table>");

        }






    }
}
