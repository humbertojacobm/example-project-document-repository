﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using DocumentDB.Repository;
using DocumentDb.Repository.Samples.Model;
using Microsoft.Azure.Documents.Client;

namespace DocumentDb.Repository.Samples
{
    internal class Program
    {
        public static BaseDocumentDbRepository<Person> Repo { get; set; }

        private static void Main(string[] args)
        {
            IDocumentDbInitializer init = new DocumentDbInitializer();

            string endpointUrl = ConfigurationManager.AppSettings["azure.documentdb.endpointUrl"];
            string authorizationKey = ConfigurationManager.AppSettings["azure.documentdb.authorizationKey"];
            string database = ConfigurationManager.AppSettings["azure.documentdb.databaseName"];

            // get the Azure DocumentDB client
            DocumentClient client = init.GetClient(endpointUrl, authorizationKey);

            // create repository for persons
            Repo = new BaseDocumentDbRepository<Person>(client, database);

            // Run demo
            Task t = MainAsync(args);
            t.Wait();
        }

        private static async Task MainAsync(string[] args)
        {
            // output all persons in our database
            await PrintPersonCollection();

            // create a new person
            Person matt = new Person
            {
                FirstName = "m4tt",
                LastName = "TBA",
                BirthDayDateTime = new DateTime(1990, 10, 10),
                PhoneNumbers =
                    new Collection<PhoneNumber>
                    {
                        new PhoneNumber {Number = "555", Type = "Mobile"},
                        new PhoneNumber {Number = "777", Type = "Landline"}
                    }
            };

            // add person to database (collection named as class name will be created by convenction, this can be configured during initialization of the repository)
            matt = await Repo.AddOrUpdateAsync(matt);

            // should output person and his two phone numbers
            await PrintPersonCollection();

            // update first name
            matt.FirstName = "Matt";

            // add last name
            matt.LastName = "Smith";

            // remove landline phone number
            matt.PhoneNumbers.RemoveAt(1);

            // should update person
            await Repo.AddOrUpdateAsync(matt);

            // should output Matt with just one phone number
            await PrintPersonCollection();

            // get Matt by his Id
            Person justMatt = await Repo.GetByIdAsync(matt.Id);
            Console.WriteLine("GetByIdAsync result: " + justMatt);

            // remove matt from collection
            await Repo.RemoveAsync(matt.Id);

            // should output nothing
            await PrintPersonCollection();
        }

        private static async Task PrintPersonCollection()
        {
            IEnumerable<Person> persons = await Repo.GetAllAsync();

            persons.ToList().ForEach(Console.WriteLine);
        }
    }
}