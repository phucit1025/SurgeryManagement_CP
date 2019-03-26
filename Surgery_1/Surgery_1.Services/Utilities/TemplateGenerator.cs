using Surgery_1.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Services.Utilities
{
    class TemplateGenerator
    {
        public static string GetHTMLString(SurgeryShift surgery)
        {
            var patient = surgery.Patient;
            var surgeryCatalog = surgery.SurgeryCatalog;
            var ekip = surgery.Ekip;
            var sb = new StringBuilder();
            sb.AppendFormat(@"<html>
                            <head>
                            </head>
                            <body>
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
                                            <td><b>Phẫu thuật/TT kết thúc lúc::</b></td>
                                            <td>{4}</td>
                                        </tr>
                                        <tr>
                                            <td><b>Chuyên khoa:</b></td>
                                            <td>{5}</td>
                                        </tr>
                                        <tr>
                                            <td><b>Tên phẫu thuật:</b></td>
                                            <td>{6}</td>
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
            , surgeryCatalog.AnesthesiaMethod);
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
                                            <td>{0}</td>
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
    }
}
