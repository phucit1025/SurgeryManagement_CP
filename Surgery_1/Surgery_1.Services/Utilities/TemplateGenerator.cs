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
                                             <td valign='top'>BỘ QUỐC PHÒNG<br>BỆNH VIỆN EBSMMS</td>
                                            <td align='center'><img style='width: 150;' src='{12}'/></td>
                                            <td align='right' valign='top'>SỐ PHIẾU PT/TT: {11}</td>
                                        </tr>
                                    </tbody>
                                    </table>
                                <div class='header' align='center'><h2>BIÊN BẢN PHẪU THUẬT</h2></div>
                                <table style='width: 100%'>
                                    <tbody>
                                        <tr>
                                            <td>Họ và tên người bệnh:</td>
                                            <td>{0}</td>
                                            <td>Giới tính:</td>
                                            <td>{1}</td>
                                            <td>Tuổi:</td>
                                            <td>{2}</td>
                                        </tr>
                                        <tr>
                                            <td>Phẫu thuật/TT lúc:</td>
                                            <td colspan='2'>{3}</td>
                                            <td> Phẫu thuật/TT kết thúc lúc:</td>
                                            <td colspan='2'>{4}</td>
                                        </tr>
                                        <tr>
                                            <td> Chuyên khoa:</td>
                                            <td colspan='5'>{5}</td>
                                        </tr>
                                        <tr>
                                            <td> Tên phẫu thuật:</td>
                                            <td colspan='5'>{10} {6}</td>
                                        <tr>
                                            <td> Thành tiền:</td>
                                            <td colspan='2'>{7}</td>
                                            <td> Loại phẫu thuật/TT:</td>
                                            <td colspan='2'>{8}</td>
                                        </tr>
                                        <tr>
                                            <td> Phương pháp vô cảm:</td>
                                            <td colspan='5'>{9}</td>
                                        </tr>
                                    </tbody>
                                </table>
                                <div class='header' align='center'><h3>EKIP THỰC HIỆN</h3></div>
                                <table align='center'>"
            , patient.FullName, patient.Gender > 0 ? "Male" : "Female", DateTime.Now.Year - patient.YearOfBirth
            , surgery.ActualStartDateTime, surgery.ActualEndDateTime
            , surgeryCatalog.Specialty.Name, surgeryCatalog.Name, surgeryCatalog.Price, surgeryCatalog.Type
            , surgeryCatalog.AnesthesiaMethod, surgeryCatalog.Code
            , surgery.Id, path);
            if (ekip != null)
            {
                foreach (var emp in ekip.Members)
                {
                    sb.AppendFormat(@"<tr>
                                    <td>{0}</td>
                                    <td></td>
                                    <td>{1}</td>
                                  </tr>", emp.WorkJob, emp.Name);
                }
            }


            sb.Append(@"
                                </table>
                                <div class='header' align='center'><h3>TRÌNH TỰ PHẪU THUẬT</h3></div>");
            if (surgery.UsedProcedure != null)
            {
                sb.AppendFormat(@"<table align='center' style='width: 100%'>
                                    <tbody>
                                        <tr>
                                            <td> </td>
                                            <td><pre style='font-family:times;'>{0}</pre></td>
                                            <td> </td>
                                            <td></td>
                                            <td> </td>
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
                                            <td> Họ và tên người bệnh:</td>
                                            <td>{0}</td>
                                            <td> Giới tính:</td>
                                            <td>{1}</td>
                                            <td> Tuổi:</td>
                                            <td>{2}</td>
                                        </tr>
                                        <tr>
                                            <td> Số giường:</td>
                                            <td colspan='2'>{3}</td>
                                            <td> Buồng:</td>
                                            <td colspan='2'>{4}</td>
                                        </tr>
                                        <tr>
                                        <td> Phẫu thuật:</td>
                                            <td colspan='5'>{5}</td>
                                         </tr>
                                    </tbody>
                                </table>
                                <div class='header' align='center'><h3></h3></div>"
            , patient.FullName, patient.Gender > 0 ? "Nam" : "Nữ", DateTime.Now.Year - patient.YearOfBirth
            , surgery.PostRoomName, surgery.PostBedName, surgeryCatalog.Name);
            sb.Append(@"<table style='border-collapse: collapse; border: 1px solid black; width:100%' align='center'>
                                   <thead>
                                        <tr>
                                            <th style=' border: 1px solid black;'>Ngày giờ</th>
                                            <th style=' border: 1px solid black;'>Theo dõi diễn biến</th>
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
