The Waste-to-Resource Exchange Management System is a cross-platform web application designed to promote sustainability through the exchange of expired or unused items such as electronics,
furniture, and household goods. Developed using a modular architecture, the system provides a responsive and user-friendly interface built with HTML, CSS, and Bootstrap, 
while backend operations and business logic are handled by an ASP.NET Web API. Data is securely stored using SQL Server, 
Core features secure user authentication, and in-app communication between donors and seekers. 
The platform enables users to list items for free or at minimal cost, making them available to individuals in need. 
By supporting efficient, secure, and community-driven exchanges, the system helps minimize waste and extend the lifecycle of usable goods.

                                                 ------------------------------------  All SQl QUERIES ------------------------------------

create database BeachDy

-- Users Table
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(100),
    Email NVARCHAR(100) UNIQUE,
    PasswordHash NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE()
);


-- Products Table
CREATE TABLE Ads (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(255) NOT NULL,
    Category NVARCHAR(100) NOT NULL,
    Price DECIMAL(18,2) NULL,
    PriceOnCall BIT NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    ImagePath NVARCHAR(500) NULL, -- Store path/URL of uploaded image

    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    Email NVARCHAR(255) NOT NULL,

    Address NVARCHAR(255) NOT NULL,
    Country NVARCHAR(100) NOT NULL,
    City NVARCHAR(100) NOT NULL,

    AgreeToTerms BIT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),

	Status NVARCHAR(50) DEFAULT 'Active', 

    UserId INT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);


CREATE TABLE AdImages (
    ImageId INT IDENTITY(1,1) PRIMARY KEY,
    AdId INT, -- Foreign key to Ads table
    ImagePath NVARCHAR(500) NOT NULL, -- Path/URL of the image
    FOREIGN KEY (AdId) REFERENCES Ads(Id) ON DELETE CASCADE
);


CREATE TABLE Favorites (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    AdId INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (AdId) REFERENCES Ads(Id)
);


-- Reviews table
CREATE TABLE Reviews (
    ReviewId INT PRIMARY KEY IDENTITY(1,1),
    AdId INT NOT NULL,            -- Kis product par review ho raha hai
    UserId INT NOT NULL,          -- Kis user ne review diya hai
    Rating INT CHECK (Rating BETWEEN 1 AND 5),  -- 1 to 5 rating
    Name NVARCHAR(100),           -- Optional: Display Name (agar required ho)
    Email NVARCHAR(100),          -- Optional: Email (form mein capture ho raha hai)
    ReviewText NVARCHAR(MAX),     -- Actual Review Content
    CreatedAt DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (AdId) REFERENCES Ads(Id),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);


CREATE TABLE Bids (
    BidId INT PRIMARY KEY IDENTITY(1,1),
    AdId INT NOT NULL,
    UserId INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    BidTime DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (AdId) REFERENCES Ads(Id),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);



CREATE TABLE AdBiddingInfo (
    Id INT PRIMARY KEY IDENTITY(1,1),
    AdId INT UNIQUE,
    StartTime DATETIME,
    EndTime DATETIME,
    IsActive BIT DEFAULT 1,

    FOREIGN KEY (AdId) REFERENCES Ads(Id)
);



--//////////////////////////////////////////////////////


-- Step 1: Drop the existing FK constraint
ALTER TABLE Favorites
DROP CONSTRAINT FK__Favorites__AdId__51300E55; -- Use correct constraint name from error

-- Step 2: Add a new FK with ON DELETE CASCADE
ALTER TABLE Favorites
ADD CONSTRAINT FK_Favorites_Ads
FOREIGN KEY (AdId) REFERENCES Ads(Id)
ON DELETE CASCADE;


--////////////////////////////////////////////////////////




drop table Ads
drop table users
drop table AdImages
drop table Favorites 

delete  Users
delete ads 
delete AdImages
delete Favorites 


SELECT * FROM AdImages WHERE AdId = 15;

--/////////////////////////////////////////////

select * from users
select * from Ads
select * from AdImages
select * from Favorites 
select * from Reviews

--/////////////////////////////////

ALTER TABLE AdBiddingInfo
ADD StartingAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
--////////////////////////////////

