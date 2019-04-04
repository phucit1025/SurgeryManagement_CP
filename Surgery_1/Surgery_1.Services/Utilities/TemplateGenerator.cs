using Surgery_1.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Services.Utilities
{
    class TemplateGenerator
    {
        public static string GetHTMLString(SurgeryShift surgery, String path)
        {
            var patient = surgery.Patient;
            var surgeryCatalog = surgery.SurgeryCatalog;
            var ekip = surgery.Ekip;
            var sb = new StringBuilder();
            sb.AppendFormat(@"<html>
                            <head>
                            </head>
                            <body>
                                    <table align='center' style='width: 100%'>
                                        <tbody>
                                        <tr>
                                             <td valign='top'>BỘ QUỐC PHÒNG<br>BỆNH VIỆN QUÂN Y 175</td>
                                            <td><img style='width: 100;' src='{12}'/></td>
                                            <td align='right' valign='top'>SỐ PHIẾU PT/TT: {11}</td>
                                        </tr>
                                    </tbody>
                                    </table>
                                <div class='header' align='center'><h2>BIÊN BẢN PHẪU THUẬT</h2></div>
                                <table align='center'>
                                    <tbody>
                                        <tr>
                                            <td><b>Họ và tên người bệnh:</b></td>
                                            <td>{0}</td>
                                            <td><b>Giới tính:</b></td>
                                            <td>{1}</td>
                                            <td><b>Tuổi:</b></td>
                                            <td>{2}</td>
                                        </tr>
                                        <tr>
                                            <td><b>Phẫu thuật/TT lúc:</b></td>
                                            <td>{3}</td>
                                            <td><b>Phẫu thuật/TT kết thúc lúc:</b></td>
                                            <td>{4}</td>
                                        </tr>
                                        <tr>
                                            <td><b>Chuyên khoa:</b></td>
                                            <td>{5}</td>
                                        </tr>
                                        <tr>
                                            <td><b>Tên phẫu thuật:</b></td>
                                            <td>{10} {6}</td>
                                        <tr>
                                            <td><b>Thành tiền:</b></td>
                                            <td>{7}</td>
                                            <td><b>Loại phẫu thuật/TT:</b></td>
                                            <td>{8}</td>
                                        </tr>
                                        <tr>
                                            <td><b>Phương pháp vô cảm:</b></td>
                                            <td>{9}</td>
                                        </tr>
                                    </tbody>
                                </table>
                                <div class='header' align='center'><h3>EKIP THỰC HIỆN</h3></div>
                                <table align='center'>"
            , patient.FullName, patient.Gender > 0 ? "Male" : "Female", DateTime.Now.Year - patient.YearOfBirth
            , surgery.ActualStartDateTime, surgery.ActualEndDateTime
            , surgeryCatalog.Speciality.Name, surgeryCatalog.Name, surgeryCatalog.Price, surgeryCatalog.Type
            , surgeryCatalog.AnesthesiaMethod, surgeryCatalog.Code
            , surgery.Id, path);
            if (ekip != null)
            {
                foreach (var emp in ekip.Members)
                {
                    sb.AppendFormat(@"<tr>
                                    <td>{0}</td>
                                    <td>{1}</td>
                                  </tr>", emp.Name, emp.WorkJob);
                }
            }


            sb.Append(@"
                                </table>
                                <div class='header' align='center'><h3>TRÌNH TỰ PHẪU THUẬT</h3></div>");
            if (surgery.UsedProcedure != null)
            {
                sb.AppendFormat(@"<table align='center'>
                                    <tbody>
                                        <tr>
                                            <td><b></b></td>
                                            <td><pre style='font-family:times;'>{0}</pre></td>
                                            <td><b></b></td>
                                            <td></td>
                                            <td><b></b></td>
                                            <td></td>
                                        </tr>
                                    </tbody>
                                </table>", surgery.UsedProcedure);
            }
            sb.Append(@"   </body>
                        </html>");
                         

            return sb.ToString();
        }

        public static string GetHTMLStringHealthcare(SurgeryShift surgery)
        {
            var patient = surgery.Patient;
            var surgeryCatalog = surgery.SurgeryCatalog;
            var ekip = surgery.Ekip;
            var sb = new StringBuilder();
            sb.AppendFormat(@"<html>
                            <head>
                            </head>
                            <body>
                                <div class='header' align='center'><h2>PHIẾU CHĂM SÓC</h2></div>
                                <table align='center' style='width: 100%'>
                                    <tbody>
                                        <tr>
                                            <td><b>Họ và tên người bệnh:</b></td>
                                            <td>{0}</td>
                                            <td><b>Giới tính:</b></td>
                                            <td>{1}</td>
                                            <td><b>Tuổi:</b></td>
                                            <td>{2}</td>
                                        </tr>
                                        <tr>
                                            <td><b>Số giường:</b></td>
                                            <td>{3}</td>
                                            <td><b>Buồng:</b></td>
                                            <td>{4}</td>
                                            <td><b>Phẫu thuật:</b></td>
                                            <td>{5}</td>
                                        </tr>
                                    </tbody>
                                </table>
                                <div class='header' align='center'><h3></h3></div>"
            , patient.FullName, patient.Gender > 0 ? "Male" : "Female", DateTime.Now.Year - patient.YearOfBirth
            , surgery.PostRoomName, surgery.PostBedName, surgeryCatalog.Name);
            sb.Append(@"<table style='border-collapse: collapse; border: 1px solid black; width:100%' align='center'>
                                   <thead>
                                        <tr>
                                            <th style=' border: 1px solid black;'>Ngày giờ</th>
                                            <th style=' border: 1px solid black;'>Theo dõi diễn biế</th>
                                            <th style=' border: 1px solid black;'>Thực hiện y lệnh/chăm sóc</th>
                                            <th style=' border: 1px solid black;'>Y tá</th>
                                        </tr>
                                    </thead><tbody>");
            if (surgery.HealthCareReports.Count > 0)
            {
                foreach (var item in surgery.HealthCareReports)
                {
                    sb.AppendFormat(@"<tr>
                                            <td style=' border: 1px solid black;'>{0}</td>
                                            <td style=' border: 1px solid black;'>{1}</td>
                                            <td style=' border: 1px solid black;'>{2}</td>
                                            <td style=' border: 1px solid black;'>{3}</td>
                                        </tr>", item.DateCreated, item.EventContent, item.CareContent, item.NurseId);
                }
               
            }
            sb.Append(@"    </tbody>
                                </table>   </body>
                        </html>");


            return sb.ToString();
        }
    }
}
