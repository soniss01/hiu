CREATE TABLE [dbo].[HIU_PatientOnFind](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	anm_id int,
	anm_name varchar(100),
	state_code int,
	[requestId] [varchar](50) NULL,
	[timestamp] [varchar](50) NULL,
	[patient_id] [varchar](20) NULL,
	[patient_name] [varchar](100) NULL,
	[error_code] [varchar](10) NULL,
	[error_message] [varchar](max) NULL,
	[resp_requestId] [varchar](50) NULL,
	[created_On] [datetime] NULL,
	[updated_On] [datetime] NULL
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


CREATE PROCEDURE [dbo].[sp_HIU_PatientOnFind_IU]
@anm_id int=0,
@anm_name varchar(100)=null,
@state_code int=0,
@requestId VARCHAR(50)=null,
@timestamp VARCHAR(50)=null,
@patient_id VARCHAR(20)=null,
@patient_name VARCHAR(100)=null,
@error_code  varchar(10)=null,
@error_message varchar(max)=null,
@resp_requestId  varchar(50)=null
AS
BEGIN
if not exists(select 1 from HIU_PatientOnFind where resp_requestId =@resp_requestId and patient_id=@patient_id )
begin
  INSERT INTO HIU_PatientOnFind
  (anm_id,anm_name,state_code, patient_id, resp_requestId, Created_On)
  VALUES
  (@anm_id,@anm_name,@state_code, @patient_id, @resp_requestId, GETDATE())
END
else
begin
update HIU_PatientOnFind set requestId=@requestId,timestamp=@timestamp,patient_name=@patient_name,error_code=@error_code,
error_message=@error_message where  resp_requestId =@resp_requestId and patient_id=@patient_id
end
end