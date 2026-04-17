using DMSA.Models.Odoo.Update;

namespace DMOrders.Pages.Sys
{
    [Obsolete("This class is obsolete. Use the new CreditNoteRequest class instead.")]
    public partial class UpdateData
    {
        public static void UpdateFiles(update_pack_info source, update_pack_info destination)
        {
            foreach (var sourceDetail in source.details)
            {
                var destDetail = destination.details.FirstOrDefault(d => d.model == sourceDetail.model);
                if (destDetail != null)
                {
                    var updatedFiles = destDetail.files.ToList();
                    foreach (var sourceFile in sourceDetail.files)
                    {
                        var destFile = updatedFiles.FirstOrDefault(f => f.name == sourceFile.name &&
                        f.year == sourceFile.year &&
                        f.month == sourceFile.month &&
                        f.day == sourceFile.day);
                        if (destFile != null)
                        {
                            if (destFile.create_date != sourceFile.create_date || destFile.hash != sourceFile.hash)
                            {
                                // Update destination file data
                                destFile.create_date = sourceFile.create_date;
                                destFile.hash = sourceFile.hash;
                                destFile.year = sourceFile.year;
                                destFile.month = sourceFile.month;
                                destFile.day = sourceFile.day;
                            }
                        }
                        else
                        {
                            // Add new file data to destination
                            updatedFiles.Add(sourceFile);
                        }
                    }
                    destDetail.files = updatedFiles.ToArray();
                    destDetail.total_files = destDetail.files.Length;
                }
                else
                {
                    // Add new detail to destination
                    var updatedDetails = destination.details.ToList();
                    updatedDetails.Add(sourceDetail);
                    destination.details = updatedDetails.ToArray();
                }
            }
        }
    }
}
