using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceListLoader {
    public static class Enums {
        public enum Cities {
            Moscow,
            SaintPetersburg,
            Sochi,
            Kazan,
            KamenskUralsky,
            Krasnodar,
            Ufa,
            Other
        }

        public enum MoscowSites {
            fdoctor_ru,
            familydoctor_ru,
            familydoctor_ru_child,
            alfazdrav_ru,
            nrmed_ru,
            nrmed_ru_child,
            nrmedlab_ru,
            onclinic_ru,
            smclinic_ru,
            smdoctor_ru,
            sm_stomatology_ru,
            smclinic_ru_lab,
            invitro_ru,
            cmd_online_ru,
            helix_ru,
            mrt24_ru,
            dentol_ru,
            zub_ru,
            vse_svoi_ru,
            novostom_ru,
            masterdent_ru,
            gemotest_ru,
            kdl_ru,
            medsi_ru,
            medsiKDCB_ru,
            medsiKPP_ru,
            medsiPIROGOVKA_ru,
            legal_entity_k31,
            legal_entity_litfond,
            legal_entity_ssmc,
            legal_entity_lechebniy_centr,
            emcmos_ru
        }

        public enum SaintPetersburgSites {
            mc21_ru,
            //evro_med_ru,
            baltzdrav_ru,
            german_clinic,
            german_dental,
            clinic_complex_ru,
            medswiss_spb_ru,
            invitro_ru,
            helix_ru,
            emcclinic_ru,
            legal_entity_allergomed,
            legal_entity_odont,
            legal_entity_pervaya_semeinaya_klinika,
            reasunmed_ru,
            dcenergo_ru,
            dcenergo_kids_ru,
            allergomed_ru,
            starsclinic_ru,
            clinica_blagodat_ru,
            med_vdk_ru
        }

        public enum SochiSites {
            armed_mc_ru,
            uzlovaya_poliklinika_ru,
            _23doc_ru_main_price,
            _23doc_ru_doctors,
            _23doc_ru_lab,
            medcentr_sochi_ru,
            kb4sochi_ru,
            medprofisochi_com,
            clinic23_ru,
            clinic23_ru_lab,
            _5vrachey_com,
            mc_daniel_ru,
            medbr_ru,
			analizy_sochi_ru
        }

        public enum KazanSites {
            ava_kazan_ru,
            mc_aybolit_ru,
            biomed_mc_ru,
            zdorovie7i_ru,
            starclinic_ru,
            love_dr_ru,
            medexpert_kazan_ru,
            kazan_clinic_ru
        }

        public enum KamenskUralskySites {
            mfcrubin_ru,
            ruslabs_ru,
            mc_vd_ru,
            immunoresurs_ru,
            medkamensk_ru,
            invitro_ru
        }

        public enum KrasnodarSites {
            clinic23_ru,
            clinic23_ru_lab,
            clinicist_ru,
            poly_clinic_ru,
            clinica_nazdorovie_ru,
            clinica_nazdorovie_ru_lab,
            kuban_kbl_ru,
            //solnechnaya,
            //v_nadezhnyh_rykah,
            //evromed,
            vrukah_com,
            vrukah_com_lab
        }

        public enum UfaSites {
            megi_clinic,
            promedicina_clinic,
            mamadeti_ru,
            rkbkuv_ru,
            mdplus_ru
        }

        public enum OtherSites {
            kovrov_clinicalcenter_ru,
            nedorezov_mc_ru,
            nedorezov_prom_metall_kz
        }
    }
}
