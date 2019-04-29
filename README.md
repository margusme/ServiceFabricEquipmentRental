This is a sample Visual Studio solution for demonstrating Service Fabric stateless and steteful services.
Solution is built with Visual Studio 2019 Professional and includes online heavy and specialized equipment rental
Each rental element along with maximum available count is first defined in file equipment.txt in the project root of EquipmentData. 

After running the solution, this data is loaded into memory and replicated across service fabric. 
User can add equipment items with days of how much he wants to rent each equipment and each item will appear inside basket. 
Items can be removed from the basket later if needed.
By closing the basket will user agree to rent chosen items and can download an invoice. 
There are buttons for reading only last invoice and if basket is generated more than once
then there will be more than one invoice and user can see all invoices by pressing on "All Invoices".

Equipment Rental example runs currently with 2 languages: English and German but new languages can be easily added later programmatically.

Serfice Fabric along with Visual Studio must be first installed in order to properly run the solution.
To run Service Fabric applications on Windows, please read https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-get-started
Visual Studio 2019 was used to generate solution but Visual Studio 2017 requirements apply to Visual Studio 2019 as well

Please download Visual Studio 2019 Professional from https://visualstudio.microsoft.com/downloads/