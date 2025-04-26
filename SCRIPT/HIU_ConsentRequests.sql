SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE dbo.HIU_ConsentRequests(
	Id bigint IDENTITY(1,1) NOT NULL,
	anm_id int,
	anm_name varchar(100),
	state_code int,
	requestId varchar(50) NULL,
	timestamp varchar(50) NULL,
	consentRequest_id varchar(50) NULL,
	error varchar(max) NULL,
	resp_requestId varchar(50) NULL,
	Created_On datetime NULL,
	Updated_On datetime NULL
) 


CREATE PROCEDURE [dbo].[sp_HIU_ConsentRequests_IU]
@anm_id int=0,
@anm_name varchar(100)=null,
@state_code int=0,
@requestId VARCHAR(50)=null,
@timestamp VARCHAR(50)=null,
@consentRequest_id  varchar(50)=null,
@error varchar(MAX)=null,
@resp_requestId  varchar(50)=null
AS
BEGIN
if not exists(select 1 from HIU_ConsentRequests where resp_requestId =@resp_requestId)
begin
  INSERT INTO HIU_ConsentRequests
  (anm_id,anm_name,state_code, resp_requestId, Created_On)
  VALUES
  (@anm_id,@anm_name,@state_code, @resp_requestId, GETDATE())
END
else
begin
   update HIU_ConsentRequests set 
   requestId=@requestId,
   timestamp=@timestamp,
   error=@error
   where  resp_requestId =@resp_requestId
end
end